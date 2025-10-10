using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.AspNetCore.Authorization;

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
        [ValidateAntiForgeryToken]
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
                    // 1. Define o caminho: wwwroot/imagens/
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string path = Path.Combine(wwwRootPath, "imagens");

                    // 2. Garante que a pasta existe
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    // 3. Gera um nome de ficheiro único (para evitar conflitos)
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(produto.ImagemUpload.FileName);
                    string filePath = Path.Combine(path, fileName);

                    // 4. Salva o ficheiro no disco
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await produto.ImagemUpload.CopyToAsync(fileStream);
                    }

                    // 5. Salva o URL relativo no Modelo (para ir para o DB)
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
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Preco,ImagemUrl,Estoque")] Produto produto)
        {
            if (id != produto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
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
