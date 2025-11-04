using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ecomerce.Services
{
    public class CarrinhoServico : ICarrinhoServico
    {
        private readonly ISession _session;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ChaveCarrinho = "CarrinhoDeSessao";

        public CarrinhoServico(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _session = httpContextAccessor.HttpContext?.Session ?? throw new InvalidOperationException("A sessão não está disponível.");
            _context = context;
        }

        private List<ItemCarrinho> ObterCarrinhoDaSessao()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return new List<ItemCarrinho>();

            var carrinhoJson = context.Session.GetString(ChaveCarrinho);

            return string.IsNullOrEmpty(carrinhoJson)
                ? new List<ItemCarrinho>()
                : JsonSerializer.Deserialize<List<ItemCarrinho>>(carrinhoJson) ?? new List<ItemCarrinho>();
        }

        private void GuardarCarrinhoNaSessao(List<ItemCarrinho> carrinho)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var carrinhoJson = JsonSerializer.Serialize(carrinho);
            context.Session.SetString(ChaveCarrinho, carrinhoJson);
        }

        public List<ItemCarrinho> ObterItens()
        {
            return ObterCarrinhoDaSessao();
        }

        public async Task<List<ItemCarrinho>> ObterDetalhesDoCarrinho()
        {
            var carrinhoItens = ObterItens();

            var variacaoIds = carrinhoItens.Select(i => i.VariacaoId).ToList();

            var variacoesComProdutos = await _context.Variacoes
                                                     .Include(v => v.Produto)
                                                     .Where(v => variacaoIds.Contains(v.Id))
                                                     .ToListAsync();

            bool houveAtualizacaoDePreco = false;

            foreach (var item in carrinhoItens)
            {
                var variacao = variacoesComProdutos.FirstOrDefault(v => v.Id == item.VariacaoId);

                if (variacao != null && variacao.Produto != null)
                {
                    item.Produto = variacao.Produto;
                    item.ProdutoId = variacao.ProdutoId;
                    item.NomeVariacao = variacao.Nome;

                    if (item.PrecoTabela != variacao.Produto.Preco || item.PrecoPromocional != variacao.Produto.PrecoPromocional)
                    {
                        item.PrecoTabela = variacao.Produto.Preco;
                        item.PrecoPromocional = variacao.Produto.PrecoPromocional > 0 ? variacao.Produto.PrecoPromocional : null;
                        houveAtualizacaoDePreco = true;
                    }
                }
                else
                {
                    item.Produto = new Produto { Nome = "Produto Indisponível" };
                    item.NomeVariacao = "Sabor Indisponível";

                    if (item.PrecoTabela != 0.00m)
                    {
                        item.PrecoTabela = 0.00m;
                        item.PrecoPromocional = null;
                        houveAtualizacaoDePreco = true;
                    }
                }
            }

            if (houveAtualizacaoDePreco)
            {
                GuardarCarrinhoNaSessao(carrinhoItens);
            }

            return carrinhoItens;
        }

        public async Task<string> AdicionarItem(int variacaoId, int quantidade)
        {
            if (variacaoId <= 0)
                return "Selecione um sabor (variação) antes de adicionar ao carrinho.";

            if (quantidade <= 0) return "Quantidade deve ser maior que zero.";

            var variacao = await _context.Variacoes
                                         .Include(v => v.Produto)
                                         .FirstOrDefaultAsync(v => v.Id == variacaoId);

            if (variacao == null || variacao.Produto == null)
                return "Variação selecionada não encontrada ou produto associado indisponível.";

            if (variacao.Estoque < quantidade)
                return $"Estoque insuficiente para a variação '{variacao.Nome}'. Disponível: {variacao.Estoque}.";

            var carrinho = ObterCarrinhoDaSessao();
            var itemExistente = carrinho.FirstOrDefault(i => i.VariacaoId == variacaoId);

            if (itemExistente != null)
            {
                if (variacao.Estoque < itemExistente.Quantidade + quantidade)
                    return $"Adição não permitida. O novo total excede o estoque ({variacao.Estoque}).";

                itemExistente.Quantidade += quantidade;

                itemExistente.PrecoTabela = variacao.Produto.Preco;
                itemExistente.PrecoPromocional = variacao.Produto.PrecoPromocional > 0 ? variacao.Produto.PrecoPromocional : null;
            }
            else
            {
                var novoProduto = new ItemCarrinho
                {
                    VariacaoId = variacaoId,
                    ProdutoId = variacao.ProdutoId,
                    Quantidade = quantidade,

                    PrecoTabela = variacao.Produto.Preco,

                    PrecoPromocional = variacao.Produto.PrecoPromocional.HasValue && variacao.Produto.PrecoPromocional.Value > 0
                        ? variacao.Produto.PrecoPromocional.Value
                        : null,

                    NomeVariacao = variacao.Nome ?? string.Empty
                };
                carrinho.Add(novoProduto);
            }

            GuardarCarrinhoNaSessao(carrinho);

            return null;
        }

        public void RemoverItem(int variacaoId)
        {
            var carrinho = ObterCarrinhoDaSessao();
            var item = carrinho.FirstOrDefault(i => i.VariacaoId == variacaoId);

            if (item != null)
            {
                carrinho.Remove(item);
                GuardarCarrinhoNaSessao(carrinho);
            }
        }

        public void LimparCarrinho()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(ChaveCarrinho);
        }

        public decimal ObterTotal(List<ItemCarrinho> itens)
        {
            decimal total = 0;

            foreach (var item in itens)
            {
                decimal precoUnitario = item.PrecoPromocional.HasValue && item.PrecoPromocional.Value > 0 ? item.PrecoPromocional.Value : item.PrecoTabela;
                total += item.Quantidade * precoUnitario;
            }

            return total - GetDescontoCupom();
        }

        public async Task<string> AplicarCupom(string codigoCupom)
        {
            if (string.IsNullOrWhiteSpace(codigoCupom))
                return "O código do cupom não pode ser vazio.";

            var cupom = await _context.Cupons.FirstOrDefaultAsync(c => c.Codigo.ToLower() == codigoCupom.ToLower());

            if (cupom == null)
                return "Cupom inválido ou não encontrado.";

            var carrinhoItens = ObterCarrinhoDaSessao();
            if (carrinhoItens == null || !carrinhoItens.Any())
                return "Seu carrinho está vazio. Adicione itens antes de aplicar um cupom.";

            decimal subtotal = carrinhoItens.Sum(item => 
            {
                decimal precoUnitario = item.PrecoPromocional.HasValue && item.PrecoPromocional.Value > 0
                    ? item.PrecoPromocional.Value
                    : item.PrecoTabela;
                return item.Quantidade * precoUnitario;
            });

            if (cupom.DataExpiracao.HasValue && cupom.DataExpiracao.Value < DateTime.Now)
                return "Este cupom está expirado.";

            if (subtotal < cupom.ValorMinimoPedido)
                return $"O cupom requer um valor mínimo de pedido de {cupom.ValorMinimoPedido:C2}.";

            decimal valorDesconto = 0.00m;

            if (cupom.TipoDesconto == TipoDesconto.Porcentagem)
                valorDesconto = subtotal * cupom.Valor / 100.00m;

            else if (cupom.TipoDesconto == TipoDesconto.Fixo)
                valorDesconto = cupom.Valor;

            else
                return "Tipo de desconto do cupom não reconhecido. Contate o suporte.";

            valorDesconto = Math.Min(valorDesconto, subtotal);

            _session.SetString("CupomCodigo", cupom.Codigo);
            _session.SetString("DescontoCupom", valorDesconto.ToString());

            return "";
        }

        public decimal GetDescontoCupom()
        {
            var descontoStr = _session.GetString("DescontoCupom");
            if (decimal.TryParse(descontoStr, out decimal desconto))
            {
                return desconto;
            }
            return 0.00m;
        }
        public string GetCodigoCupom()
        {
            return _session.GetString("CupomCodigo");
        }

        public void RemoverCupom()
        {
            _session.Remove("CupomCodigo");
            _session.Remove("DescontoCupom");
        }

        public decimal GetTotalPedido()
        {
            var carrinhoItens = ObterCarrinhoDaSessao();
            if (carrinhoItens == null || !carrinhoItens.Any()) return 0.00m;

            decimal subtotalBase = carrinhoItens.Sum(item =>
            {
                decimal precoUnitario = item.PrecoPromocional.HasValue && item.PrecoPromocional.Value > 0
                    ? item.PrecoPromocional.Value
                    : item.PrecoTabela;
                    
                return item.Quantidade * precoUnitario;
            });

            decimal descontoCupom = GetDescontoCupom();
            decimal total = subtotalBase - descontoCupom;

            return Math.Max(0, total);
        }
    }
}