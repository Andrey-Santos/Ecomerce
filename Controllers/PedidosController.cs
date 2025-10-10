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

    [Authorize]
    [HttpGet]
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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AguardandoSaida()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Usuario)
            .Where(p => p.Status == "Processando")
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();

        ViewData["Title"] = "Pedidos Aguardando Saída";

        return View(pedidos);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DetalhesAdmin(int? id)
    {
        if (id == null)
            return NotFound();

        // Carrega o Pedido, o Usuário, os Itens do Pedido e o Produto de cada Item
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.ItensPedido)
                .ThenInclude(ip => ip.Produto)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (pedido == null)
            return NotFound();

        ViewData["StatusList"] = new List<string> { "Processando", "Enviado", "Entregue", "Cancelado" };

        return View(pedido);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarStatus(int id, string novoStatus)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
        {
            TempData["Error"] = "Pedido não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        // Altera o status e salva
        pedido.Status = novoStatus;
        _context.Update(pedido);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Status do Pedido #{id} alterado para **{novoStatus}** com sucesso!";
        return RedirectToAction(nameof(DetalhesAdmin), new { id = id });
    }
}