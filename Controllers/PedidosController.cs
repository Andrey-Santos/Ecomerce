using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Ecomerce.Data;
using Ecomerce.Services;
using Ecomerce.Models;

namespace Ecomerce.Controllers;

[Authorize] 
public class PedidosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICarrinhoServico _carrinhoServico;

    public PedidosController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ICarrinhoServico carrinhoServico)
    {
        _context = context;
        _userManager = userManager;
        _carrinhoServico = carrinhoServico;
    }

    // GET: Pedidos/MeusPedidos
    public async Task<IActionResult> MeusPedidos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var pedidos = await _context.Pedidos
            .Where(p => p.UsuarioId == userId)
            .Include(p => p.ItensPedido)
                .ThenInclude(ip => ip.Produto) 
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();

        return View(pedidos);
    }
}