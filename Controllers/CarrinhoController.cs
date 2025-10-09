using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Services;

namespace Ecomerce.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoServico _carrinhoServico;
        private readonly ApplicationDbContext _context;

        public CarrinhoController(ICarrinhoServico carrinhoServico, ApplicationDbContext context)
        {
            _carrinhoServico = carrinhoServico;
            _context = context;
        }

        public IActionResult Index()
        {
            var itensSessao = _carrinhoServico.ObterItens();
            var itensCarrinhoComDetalhes = new List<ItemCarrinho>();

            var produtoIds = itensSessao.Select(i => i.ProdutoId).ToList();
            var produtosDb = _context.Produtos.Where(p => produtoIds.Contains(p.Id)).ToList();

            foreach (var itemSessao in itensSessao)
            {
                var produtoDetalhes = produtosDb.FirstOrDefault(p => p.Id == itemSessao.ProdutoId);

                if (produtoDetalhes != null)
                {
                    itensCarrinhoComDetalhes.Add(new ItemCarrinho
                    {
                        ProdutoId = itemSessao.ProdutoId,
                        Quantidade = itemSessao.Quantidade,
                        Produto = produtoDetalhes
                    });
                }
            }

            ViewData["CarrinhoTotal"] = _carrinhoServico.ObterTotal(itensSessao);
            return View(itensCarrinhoComDetalhes);
        }

        [HttpPost]
        public IActionResult Adicionar(int id, int quantidade = 1)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            _carrinhoServico.AdicionarItem(id, quantidade);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remover(int id)
        {
            _carrinhoServico.RemoverItem(id);
            return RedirectToAction("Index");
        }
    }
}