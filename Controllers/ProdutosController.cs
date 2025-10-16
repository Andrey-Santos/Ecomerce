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
            return View(await _context.Produtos.ToListAsync());
        }

        // GET: Produtos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos.FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,Preco,Estoque,ImagemUpload")] Produto produto)
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

                // 6. Salva o Produto no DB
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Preco,Estoque,CategoriaId,ImagemUrl,ImagemUpload")] Produto produto)
        {
            if (id != produto.Id)
                return NotFound();

            var produtoOriginal = await _context.Produtos
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produtoOriginal == null)
                return NotFound();

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
            return RedirectToAction(nameof(Index));
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

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }
    }
}
