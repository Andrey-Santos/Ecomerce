using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Ecomerce.Services;
using System.Security.Claims;
using Ecomerce.Data;
using Ecomerce.Models;
using Ecomerce.Extensoes;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICarrinhoServico _carrinhoServico;

    public CheckoutController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ICarrinhoServico carrinhoServico)
    {
        _context = context;
        _userManager = userManager;
        _carrinhoServico = carrinhoServico;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var carrinhoItens = await _carrinhoServico.ObterDetalhesDoCarrinho();

        if (!carrinhoItens.Any())
        {
            TempData.Put("Notificacao", new Notificacao
            {
                Tipo = "Success",
                Mensagem = "Seu carrinho está vazio."
            });

            return RedirectToAction("Index", "Home");
        }

        ViewBag.CarrinhoItens = carrinhoItens;
        ViewBag.TotalCarrinho = carrinhoItens != null ? _carrinhoServico.ObterTotal(carrinhoItens) : 0.00m;

        return View(new Pedido());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([Bind("NomeCliente,Endereco,Cidade")] Pedido pedido)
    {
        var carrinhoItens = await _carrinhoServico.ObterDetalhesDoCarrinho();
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CarrinhoItens = carrinhoItens;
                ViewBag.TotalCarrinho = carrinhoItens != null ? _carrinhoServico.ObterTotal(carrinhoItens) : 0.00m;
                return View(pedido);
            }

            if (!carrinhoItens.Any())
            {
                TempData.Put("Notificacao", new Notificacao
                {
                    Tipo = "Success",
                    Mensagem = "Erro: Carrinho vazio."
                });

                return RedirectToAction("Index", "Home");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Usuário não autenticado.");
            decimal totalPedido = carrinhoItens != null ? _carrinhoServico.ObterTotal(carrinhoItens) : 0.00m;

            var novoPedido = new Pedido
            {
                UsuarioId = userId,
                DataPedido = DateTime.Now,
                TotalPedido = totalPedido,
                Status = "Processando",
                // Dados de Endereço do Formulário
                NomeCliente = pedido.NomeCliente,
                Endereco = pedido.Endereco,
                Cidade = pedido.Cidade
            };

            _context.Pedidos.Add(novoPedido);
            await _context.SaveChangesAsync();

            foreach (ItemCarrinho itemCarrinho in carrinhoItens)
            {
                if (itemCarrinho.Produto == null)
                    continue;

                var itemPedido = new ItemPedido
                {
                    PedidoId = novoPedido.Id,
                    ProdutoId = itemCarrinho.Produto!.Id,
                    Quantidade = itemCarrinho.Quantidade,
                    PrecoUnitario = itemCarrinho.Produto.Preco
                };
                _context.ItensPedido.Add(itemPedido);

                var produto = await _context.Produtos.FindAsync(itemCarrinho.Produto.Id);
                if (produto != null)
                {
                    produto.Estoque -= itemCarrinho.Quantidade;
                    if (produto.Estoque < 0) produto.Estoque = 0;
                    _context.Update(produto);
                }
            }

            TempData.Put("Notificacao", new Notificacao
            {
                Tipo = "Success",
                Mensagem = $"Pedido #{novoPedido.Id} realizado com sucesso!"
            });

        }
        catch
        {
            _context.ChangeTracker.Clear();
        }
        finally
        {
            await _context.SaveChangesAsync();
            _carrinhoServico.LimparCarrinho();
        }

        return RedirectToAction("MeusPedidos", "Pedidos");
    }
}