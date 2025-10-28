using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Services;

namespace Ecomerce.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoServico _carrinhoServico;

        public CarrinhoController(ICarrinhoServico carrinhoServico, ApplicationDbContext context)
        {
            _carrinhoServico = carrinhoServico;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _carrinhoServico.ObterDetalhesDoCarrinho());
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar(int variacaoId, int quantidade = 1)
        {
            var erro = await _carrinhoServico.AdicionarItem(variacaoId, quantidade);
            if (erro != null)
                return Json(new { success = false, message = erro });

            return Json(new
            {
                success = true,
                message = "Sabor adicionado ao carrinho com sucesso!",
                totalItens = _carrinhoServico.ObterItens().Count()
            });
        }

        [HttpPost]
        public IActionResult Remover(int variacaoId)
        {
            _carrinhoServico.RemoverItem(variacaoId);
            return RedirectToAction("Index");
        }
    }
}