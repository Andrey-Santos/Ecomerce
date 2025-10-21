using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Ecomerce.Services;
using System.Security.Claims;
using Ecomerce.Data;
using Ecomerce.Models;
using Ecomerce.Extensoes;
using Microsoft.EntityFrameworkCore;

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

        if (!ModelState.IsValid || !carrinhoItens.Any())
        {
            if (!carrinhoItens.Any())
            {
                TempData.Put("Notificacao", new Notificacao
                {
                    Tipo = "Error",
                    Mensagem = "Erro: Carrinho vazio. Adicione itens antes de finalizar a compra."
                });
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CarrinhoItens = carrinhoItens;
            ViewBag.TotalCarrinho = _carrinhoServico.ObterTotal(carrinhoItens);
            return View(pedido);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Usuário não autenticado.");

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var variacaoIds = carrinhoItens.Select(i => i.VariacaoId).ToList();
                var variacoesDb = await _context.Variacoes
                                                .Where(v => variacaoIds.Contains(v.Id))
                                                .ToListAsync();

                foreach (var itemCarrinho in carrinhoItens)
                {
                    var variacao = variacoesDb.FirstOrDefault(v => v.Id == itemCarrinho.VariacaoId);

                    if (variacao == null)
                        throw new InvalidOperationException($"Variação ID {itemCarrinho.VariacaoId} não encontrada no banco de dados.");

                    if (variacao.Estoque < itemCarrinho.Quantidade)
                    {
                        TempData.Put("Notificacao", new Notificacao
                        {
                            Tipo = "Error",
                            Mensagem = $"Estoque insuficiente para o sabor '{variacao.Nome}'. Disponível: {variacao.Estoque}."
                        });

                        return RedirectToAction("Index");
                    }

                    variacao.Estoque -= itemCarrinho.Quantidade;
                    _context.Variacoes.Update(variacao);
                }

                decimal totalPedido = _carrinhoServico.ObterTotal(carrinhoItens);
                var novoPedido = new Pedido
                {
                    UsuarioId = userId,
                    DataPedido = DateTime.Now,
                    TotalPedido = totalPedido,
                    Status = "Processando",
                    NomeCliente = pedido.NomeCliente,
                    Endereco = pedido.Endereco,
                    Cidade = pedido.Cidade
                };

                _context.Pedidos.Add(novoPedido);
                await _context.SaveChangesAsync();

                foreach (ItemCarrinho itemCarrinho in carrinhoItens)
                {
                    var produto = itemCarrinho.Produto;
                    var variacao = variacoesDb.FirstOrDefault(v => v.Id == itemCarrinho.VariacaoId);

                    if (produto == null || variacao == null) continue;

                    var itemPedido = new ItemPedido
                    {
                        PedidoId = novoPedido.Id,
                        ProdutoId = produto.Id,
                        VariacaoId = itemCarrinho.VariacaoId,
                        Quantidade = itemCarrinho.Quantidade,
                        PrecoUnitario = produto.Preco
                    };
                    _context.ItensPedido.Add(itemPedido);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _carrinhoServico.LimparCarrinho();

                TempData.Put("Notificacao", new Notificacao
                {
                    Tipo = "Success",
                    Mensagem = $"Pedido #{novoPedido.Id} realizado com sucesso!"
                });

                return RedirectToAction("MeusPedidos", "Pedidos");
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData.Put("Notificacao", new Notificacao
                {
                    Tipo = "Error",
                    Mensagem = "Ocorreu um erro ao finalizar o pedido. Tente novamente mais tarde."
                });

                _context.ChangeTracker.Clear();

                ViewBag.CarrinhoItens = carrinhoItens;
                ViewBag.TotalCarrinho = _carrinhoServico.ObterTotal(carrinhoItens);
                return View(pedido);
            }
        }
    }
}