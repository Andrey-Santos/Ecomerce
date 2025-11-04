using Ecomerce.Data;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Services;

namespace Ecomerce.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoServico _carrinhoServico;

        public CarrinhoController(ICarrinhoServico carrinhoServico)
        {
            _carrinhoServico = carrinhoServico;
        }

        public async Task<IActionResult> Index()
        {
            if (TempData["MensagemCupom"] != null)
            {
                ViewBag.MensagemCupom = TempData["MensagemCupom"];
            }
            
            return View(await _carrinhoServico.ObterDetalhesDoCarrinho());
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar(int variacaoId, int quantidade = 1)
        {
            var erro = await _carrinhoServico.AdicionarItem(variacaoId, quantidade);
            if (erro != null)
                return Json(new { success = false, message = erro });

            int totalItens = _carrinhoServico.ObterItens().Sum(i => i.Quantidade);
            
            string mensagemSucesso =  $"Item adicionado ao carrinho!";

            return Json(new
            {
                success = true,
                message = mensagemSucesso,
                totalItens = totalItens
            });
        }

        [HttpPost]
        public IActionResult RemoverItem(int variacaoId)
        {
            _carrinhoServico.RemoverItem(variacaoId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AplicarCupom(string codigoCupom)
        {
            string mensagem = await _carrinhoServico.AplicarCupom(codigoCupom);
            
            TempData["MensagemCupom"] = mensagem;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoverCupom()
        {
            _carrinhoServico.RemoverCupom();
            
            return RedirectToAction("Index");
        }
    }
}