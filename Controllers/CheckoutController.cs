using Ecomerce.Data;
using Ecomerce.Models;
using Ecomerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// ... (usings)

// Apenas utilizadores autenticados podem aceder a este controller
[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICarrinhoServico _carrinhoServico;
    private readonly UserManager<IdentityUser> _userManager;

    // Construtor... (Não Mudar!)
    public CheckoutController(
        ApplicationDbContext context,
        ICarrinhoServico carrinhoServico,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _carrinhoServico = carrinhoServico;
        _userManager = userManager;
    }

    // [GET] Index... (Não Mudar!)
    public IActionResult Index()
    {
        var carrinhoItens = _carrinhoServico.ObterItens();

        if (!carrinhoItens.Any())
        {
            TempData["MensagemErro"] = "O carrinho está vazio. Adicione produtos antes de finalizar a compra.";
            return RedirectToAction("Index", "Carrinho");
        }

        return View(new Encomenda());
    }

    [HttpPost]
    public async Task<IActionResult> Index(Encomenda encomenda)
    {
        var carrinhoItens = _carrinhoServico.ObterItens();

        if (!carrinhoItens.Any())
        {
            ModelState.AddModelError("", "O carrinho está vazio.");
        }

        if (!ModelState.IsValid)
            return View(encomenda);

        var userId = _userManager.GetUserId(User);
        var total  = _carrinhoServico.ObterTotal(carrinhoItens);

        encomenda.UtilizadorId = userId;
        encomenda.DataEncomenda = DateTime.Now;
        encomenda.EncomendaTotal = total;

        var produtoIds = carrinhoItens.Select(i => i.ProdutoId).ToList();
        var produtos = _context.Produtos.Where(p => produtoIds.Contains(p.Id)).ToList();

        encomenda.ItensEncomenda = new List<ItemEncomenda>();

        foreach (var itemCarrinho in carrinhoItens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == itemCarrinho.ProdutoId);

            if (produto == null || produto.Estoque < itemCarrinho.Quantidade)
            {
                ModelState.AddModelError("", $"Estoque insuficiente para {produto?.Nome ?? "Produto Desconhecido"}.");
                return View(encomenda);
            }

            produto.Estoque -= itemCarrinho.Quantidade;

            var itemEncomenda = new ItemEncomenda
            {
                ProdutoId = produto.Id,
                Quantidade = itemCarrinho.Quantidade,
                PrecoNaCompra = produto.Preco
            };

            encomenda.ItensEncomenda.Add(itemEncomenda);
        }

        _context.Encomendas.Add(encomenda);
        await _context.SaveChangesAsync();

        _carrinhoServico.LimparCarrinho();
        return RedirectToAction("Confirmacao", new { id = encomenda.Id });
    }

    public IActionResult Confirmacao(int id)
    {
        ViewData["EncomendaId"] = id;
        return View();
    }
}