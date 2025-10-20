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
            var itensCarrinhoComDetalhes = await _carrinhoServico.ObterDetalhesDoCarrinho();

            return View(itensCarrinhoComDetalhes);
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar(int variacaoId, int quantidade = 1)
        {
            if (variacaoId <= 0)
                return Json(new { success = false, message = "Selecione um sabor (variação) antes de adicionar ao carrinho." });

            if (quantidade <= 0)
                return Json(new { success = false, message = "A quantidade deve ser maior que zero." });

            var erro = await _carrinhoServico.AdicionarItem(variacaoId, quantidade);

            if (erro == null)
            {
                return Json(new
                {
                    success = true,
                    message = "Sabor adicionado ao carrinho com sucesso!",
                    totalItens = _carrinhoServico.ObterItens().Count()
                });
            }
            else
            {
                return Json(new { success = false, message = erro });
            }
        }

        [HttpPost]
        public IActionResult Remover(int id)
        {
            _carrinhoServico.RemoverItem(id);
            return RedirectToAction("Index");
        }
    }
}