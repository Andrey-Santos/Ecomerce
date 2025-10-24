
using Ecomerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecomerce.ViewComponents
{
    public class CarrosselPromocaoViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CarrosselPromocaoViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var produtos = await _context.Produtos
                                         .Where(p => p.EmPromocao == true && p.PrecoPromocional.HasValue)
                                         .OrderByDescending(p => p.Id)
                                         .Take(5)
                                         .ToListAsync();

            return View(produtos);
        }
    }
}