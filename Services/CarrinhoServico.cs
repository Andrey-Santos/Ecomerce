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

            foreach (var item in carrinhoItens)
            {
                var variacao = variacoesComProdutos.FirstOrDefault(v => v.Id == item.VariacaoId);

                if (variacao != null && variacao.Produto != null)
                {
                    item.Produto = variacao.Produto;
                    item.ProdutoId = variacao.ProdutoId;
                    item.NomeVariacao = variacao.Nome;
                }
                else
                {
                    item.Produto = new Produto { Nome = "Produto Indisponível" };
                    item.NomeVariacao = "Sabor Indisponível";
                }
            }

            return carrinhoItens;
        }

        public async Task<string> AdicionarItem(int variacaoId, int quantidade)
        {
            if (quantidade <= 0) return "Quantidade deve ser maior que zero.";

            var variacao = await _context.Variacoes
                                         .Include(v => v.Produto)
                                         .FirstOrDefaultAsync(v => v.Id == variacaoId);

            if (variacao == null || variacao.Produto == null)
                return "Sabor selecionado não encontrado ou produto associado indisponível.";

            if (variacao.Estoque < quantidade)
                return $"Estoque insuficiente para o sabor '{variacao.Nome}'. Disponível: {variacao.Estoque}.";

            var carrinho = ObterCarrinhoDaSessao();
            var itemExistente = carrinho.FirstOrDefault(i => i.VariacaoId == variacaoId);

            if (itemExistente != null)
            {
                if (variacao.Estoque < itemExistente.Quantidade + quantidade)
                    return $"Adição não permitida. O novo total excede o estoque ({variacao.Estoque}).";

                itemExistente.Quantidade += quantidade;
            }
            else
            {
                var novoProduto = new ItemCarrinho
                {
                    VariacaoId = variacaoId,
                    ProdutoId = variacao.ProdutoId,
                    Quantidade = quantidade,
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

            var variacaoIds = itens.Select(i => i.VariacaoId).ToList();

            var variacoesComProdutos = _context.Variacoes
                                               .Include(v => v.Produto)
                                               .Where(v => variacaoIds.Contains(v.Id))
                                               .ToList();

            foreach (var item in itens)
            {
                var variacao = variacoesComProdutos.FirstOrDefault(v => v.Id == item.VariacaoId);

                if (variacao != null && variacao.Produto != null)
                    total += item.Quantidade *  variacao.Produto.PrecoPromocional ?? variacao.Produto.Preco;
            }

            return total;
        }
    }
}