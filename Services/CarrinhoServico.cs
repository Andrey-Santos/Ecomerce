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

            foreach (var item in carrinhoItens)
                item.Produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId);

            return carrinhoItens;
        }

        public void AdicionarItem(int produtoId, int quantidade)
        {
            var carrinho = ObterCarrinhoDaSessao();
            var item = carrinho.FirstOrDefault(i => i.ProdutoId == produtoId);

            if (item != null)
            {
                item.Quantidade += quantidade;
            }
            else
            {
                carrinho.Add(new ItemCarrinho { ProdutoId = produtoId, Quantidade = quantidade });
            }

            GuardarCarrinhoNaSessao(carrinho);
        }

        public void RemoverItem(int produtoId)
        {
            var carrinho = ObterCarrinhoDaSessao();
            var item = carrinho.FirstOrDefault(i => i.ProdutoId == produtoId);

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
            
            var produtoIds = itens.Select(i => i.ProdutoId).ToList();
            var produtos = _context.Produtos.Where(p => produtoIds.Contains(p.Id)).ToList();
            
            foreach (var item in itens)
            {
                var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
                if (produto != null)
                {
                    total += produto.Preco * item.Quantidade;
                }
            }
            
            return total;
        }
    }
};