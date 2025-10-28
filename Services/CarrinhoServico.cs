using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ecomerce.Services
{
    public class CarrinhoServico : ICarrinhoServico
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ChaveCarrinho = "CarrinhoDeSessao";

        public CarrinhoServico(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
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

            return total;
        }
    }
}