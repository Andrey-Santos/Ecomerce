using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Ecomerce.Services;
using System.Security.Claims;
using Ecomerce.Data;
using Ecomerce.Models;
using Ecomerce.Extensoes;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;

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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Usuário não autenticado.");
            
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
                
                decimal totalFinal = _carrinhoServico.GetTotalPedido();
                string codigoCupom = _carrinhoServico.GetCodigoCupom();
                decimal valorDescontoCupom = _carrinhoServico.GetDescontoCupom();
                
                var novoPedido = new Pedido
                {
                    UsuarioId = userId,
                    DataPedido = DateTime.Now,
                    TotalPedido = totalFinal,
                    Status = "Processando",
                    NomeCliente = pedido.NomeCliente,
                    Endereco = pedido.Endereco,
                    Cidade = pedido.Cidade,
                    
                    CodigoCupom = codigoCupom,
                    ValorDescontoCupom = valorDescontoCupom
                };

                _context.Pedidos.Add(novoPedido);
                await _context.SaveChangesAsync();

                foreach (ItemCarrinho itemCarrinho in carrinhoItens)
                {
                    decimal precoFinalCobrado = itemCarrinho.PrecoPromocional.HasValue && itemCarrinho.PrecoPromocional.Value > 0
                        ? itemCarrinho.PrecoPromocional.Value
                        : itemCarrinho.PrecoTabela;

                    var itemPedido = new ItemPedido
                    {
                        PedidoId = novoPedido.Id,
                        ProdutoId = itemCarrinho.ProdutoId,
                        VariacaoId = itemCarrinho.VariacaoId,
                        Quantidade = itemCarrinho.Quantidade,
                        
                        PrecoTabela = itemCarrinho.PrecoTabela,
                        PrecoPromocional = itemCarrinho.PrecoPromocional,
                        
                        PrecoUnitario = precoFinalCobrado
                    };
                    _context.ItensPedido.Add(itemPedido);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                const string NUMERO_WHATSAPP_ADMIN = "5547988762959"; 
                
                var cultureInfo = new CultureInfo("pt-BR");
                var resumoTexto = new StringBuilder();

                resumoTexto.AppendLine($"*🛒 NOVO PEDIDO RECEBIDO #@{novoPedido.Id}*");
                resumoTexto.AppendLine($"Data: {novoPedido.DataPedido.ToString("dd/MM/yyyy HH:mm")}");
                resumoTexto.AppendLine($"---");
                resumoTexto.AppendLine($"*Cliente:* {novoPedido.NomeCliente}");
                resumoTexto.AppendLine($"*Endereço:* {novoPedido.Endereco}");
                resumoTexto.AppendLine($"*Cidade:* {novoPedido.Cidade}");
                resumoTexto.AppendLine($"---");
                resumoTexto.AppendLine($"*ITENS DO PEDIDO:*");
                
                foreach (var item in carrinhoItens)
                {
                    resumoTexto.AppendLine($"- {item.Quantidade}x {item.Produto.Nome} ({item.NomeVariacao})");
                }
                
                resumoTexto.AppendLine($"---");
                resumoTexto.AppendLine($"*TOTAL FINAL: {novoPedido.TotalPedido.ToString("C", cultureInfo)}*");
                
                if (novoPedido.ValorDescontoCupom > 0)
                {
                    resumoTexto.AppendLine($"Desconto (Cupom: {novoPedido.CodigoCupom}): -{novoPedido.ValorDescontoCupom.ToString("C", cultureInfo)}");
                }
                resumoTexto.AppendLine($"---");
                resumoTexto.AppendLine("Acesse o painel para gerenciar o pedido.");

                string mensagemCodificada = Uri.EscapeDataString(resumoTexto.ToString());
                string whatsappUrl = $"https://wa.me/{NUMERO_WHATSAPP_ADMIN}?text={mensagemCodificada}";
                
                _carrinhoServico.LimparCarrinho();
                _carrinhoServico.RemoverCupom();

                TempData.Put("Notificacao", new Notificacao
                {
                    Tipo = "Success",
                    Mensagem = $"Pedido #{novoPedido.Id} realizado com sucesso! Você será redirecionado para confirmar no WhatsApp."
                });

                return Redirect(whatsappUrl);
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