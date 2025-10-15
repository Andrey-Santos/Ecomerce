using Ecomerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecomerce.ViewComponents
{
    public class CarrinhoResumoViewComponent : ViewComponent
    {
        private readonly ICarrinhoServico _carrinhoServico;

        public CarrinhoResumoViewComponent(ICarrinhoServico carrinhoServico)
        {
            _carrinhoServico = carrinhoServico;
        }

        public IViewComponentResult Invoke()
        {
            var itens = _carrinhoServico.ObterItens();

            int totalItens = itens.Count();

            return View(totalItens);
        }
    }
}