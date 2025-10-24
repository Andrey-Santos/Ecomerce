using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.AspNetCore.Authorization;
using Ecomerce.Extensoes;

namespace Ecomerce.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProdutosController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var produto = await _context.Produtos
                                        .Include(p => p.Variacoes)
                                        .ToListAsync();

            return View(produto);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Variacoes.Where(v => v.Estoque > 0))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,Preco,ImagemUpload")] Produto produto)
        {
            ModelState.Remove("ImagemUrl");

            if (ModelState.IsValid)
            {
                if (produto.ImagemUpload != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string path = Path.Combine(wwwRootPath, "imagens");

                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(produto.ImagemUpload.FileName);
                    string filePath = Path.Combine(path, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await produto.ImagemUpload.CopyToAsync(fileStream);

                    produto.ImagemUrl = "/imagens/" + fileName;
                }

                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("GerenciarVariacoes", new { id = produto.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }
            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Preco,CategoriaId,ImagemUrl,ImagemUpload,PrecoPromocional,EmPromocao")] Produto produto)
        {
            if (id != produto.Id)
                return NotFound();

            var produtoOriginal = await _context.Produtos
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (produtoOriginal == null)
                return NotFound();

            if (produto.EmPromocao && !produto.PrecoPromocional.HasValue)
                ModelState.AddModelError("PrecoPromocional", "O preço promocional é obrigatório se a opção 'Destaque na Home' estiver marcada.");

            if (!ModelState.IsValid)
                return View(produto);

            try
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;

                if (produto.ImagemUpload != null)
                {
                    string uploadPath = Path.Combine(wwwRootPath, "imagens");
                    string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(produto.ImagemUpload.FileName);
                    string filePath = Path.Combine(uploadPath, nomeArquivo);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await produto.ImagemUpload.CopyToAsync(fileStream);

                    produto.ImagemUrl = $"/imagens/{nomeArquivo}";

                    if (!string.IsNullOrEmpty(produtoOriginal.ImagemUrl))
                    {
                        var caminhoAntigo = Path.Combine(wwwRootPath, produtoOriginal.ImagemUrl.TrimStart('/'));

                        if (System.IO.File.Exists(caminhoAntigo))
                        {
                            System.IO.File.Delete(caminhoAntigo);
                        }
                    }
                }
                else
                    produto.ImagemUrl = produtoOriginal.ImagemUrl;

                _context.Update(produto);
                await _context.SaveChangesAsync();

                TempData.Put("Notificacao", new Notificacao { Tipo = "Success", Mensagem = $"Produto '{produto.Nome}' atualizado com sucesso." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Produtos.Any(e => e.Id == produto.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("GerenciarVariacoes", new { id = produto.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GerenciarVariacoes(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                                        .Include(p => p.Variacoes)
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null)
                return NotFound();

            ViewData["ProdutoNome"] = produto.Nome;

            return View(produto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarVariacoes(Produto produto, string action)
        {
            var produtoAtual = await _context.Produtos
                .Include(p => p.Variacoes)
                .FirstOrDefaultAsync(p => p.Id == produto.Id);

            if (produtoAtual == null)
            {
                TempData.Put("Notificacao", new Notificacao { Tipo = "Error", Mensagem = "Produto mestre não encontrado." });
                return RedirectToAction(nameof(Index));
            }

            if (action.StartsWith("delete-"))
            {
                var variacaoIdParaDeletar = int.Parse(action.Split('-')[1]);

                var variacaoParaRemover = produtoAtual.Variacoes.FirstOrDefault(v => v.Id == variacaoIdParaDeletar);

                if (variacaoParaRemover != null)
                {
                    _context.Variacoes.Remove(variacaoParaRemover);
                    await _context.SaveChangesAsync();
                    TempData.Put("Notificacao", new Notificacao { Tipo = "Success", Mensagem = "Sabor excluído com sucesso." });
                }
                else
                {
                    TempData.Put("Notificacao", new Notificacao { Tipo = "Error", Mensagem = "Sabor não encontrado para exclusão." });
                }

                return RedirectToAction(nameof(GerenciarVariacoes), new { id = produto.Id });
            }


            if (produto.Variacoes == null)
            {
                TempData.Put("Notificacao", new Notificacao { Tipo = "Warning", Mensagem = "Nenhuma variação enviada para processamento." });
                return RedirectToAction(nameof(GerenciarVariacoes), new { id = produto.Id });
            }

            foreach (var variacaoEnviada in produto.Variacoes.Where(v => v.Nome != null))
            {
                variacaoEnviada.ProdutoId = produto.Id;

                if (variacaoEnviada.Id == 0)
                {
                    if (action == "add")
                        produtoAtual.Variacoes.Add(variacaoEnviada);
                }
                else
                {
                    var variacaoExistente = produtoAtual.Variacoes.FirstOrDefault(v => v.Id == variacaoEnviada.Id);

                    if (variacaoExistente != null)
                    {
                        variacaoExistente.Nome = variacaoEnviada.Nome;
                        variacaoExistente.Estoque = variacaoEnviada.Estoque;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData.Put("Notificacao", new Notificacao { Tipo = "Success", Mensagem = "Variações salvas com sucesso." });

            // Redireciona de volta para a tela de gerenciamento
            return RedirectToAction(nameof(GerenciarVariacoes), new { id = produto.Id });
        }
    }
}