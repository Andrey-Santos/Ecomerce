using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Models;
using Ecomerce.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecomerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchString)
    {
        var produtosQuery = _context.Produtos
            .Where(p => p.Estoque > 0)
            .AsQueryable();

        if (!String.IsNullOrEmpty(searchString))
        {
            // Filtra por Nome OU Descrição. Usando Contains para LIKE do SQL
            produtosQuery = produtosQuery.Where(p => 
                p.Nome.Contains(searchString) || 
                p.Descricao.Contains(searchString));
        }

        var produtos = await produtosQuery
            .OrderBy(p => p.Nome)
            .ToListAsync();
        
        // Salva o termo de busca para preencher o input na View
        ViewData["CurrentFilter"] = searchString;

        return View(produtos);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
