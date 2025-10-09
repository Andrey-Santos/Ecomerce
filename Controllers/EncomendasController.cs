using Ecomerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class EncomendasController : Controller
{
    private readonly ApplicationDbContext _context;

    public EncomendasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? dataInicial, DateTime? dataFinal)
    {
        var encomendasQuery = _context.Encomendas.AsQueryable();

        if (dataInicial.HasValue)
        {
            encomendasQuery = encomendasQuery.Where(e => e.DataEncomenda.Date >= dataInicial.Value.Date);
        }

        if (dataFinal.HasValue)
        {
            encomendasQuery = encomendasQuery.Where(e => e.DataEncomenda.Date <= dataFinal.Value.Date);
        }

        var encomendas = await encomendasQuery
            .Include(e => e.ItensEncomenda)
            .OrderByDescending(e => e.DataEncomenda)
            .ToListAsync();

        // Passa os valores para a View
        ViewData["DataInicial"] = dataInicial?.ToString("yyyy-MM-dd");
        ViewData["DataFinal"] = dataFinal?.ToString("yyyy-MM-dd");

        return View(encomendas);
    }
}