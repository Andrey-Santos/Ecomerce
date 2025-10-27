using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecomerce.Data;
using Ecomerce.Models;
using Microsoft.AspNetCore.Authorization;

namespace Ecomerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CuponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Cupons
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cupons.ToListAsync());
        }

        // GET: Admin/Cupons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cupom == null)
            {
                return NotFound();
            }

            return View(cupom);
        }

        // GET: Admin/Cupons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Cupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,TipoDesconto,Valor,ValorMinimoPedido,Ativo,DataExpiracao,LimiteUsos")] Cupom cupom)
        {
            if (ModelState.IsValid)
            {
                if (_context.Cupons.Any(c => c.Codigo == cupom.Codigo))
                {
                    ModelState.AddModelError("Codigo", "Este código de cupom já existe.");
                    return View(cupom);
                }

                cupom.UsosAtuais = 0;
                
                _context.Add(cupom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cupom);
        }

        // GET: Admin/Cupons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null)
            {
                return NotFound();
            }
            return View(cupom);
        }

        // POST: Admin/Cupons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,TipoDesconto,Valor,ValorMinimoPedido,Ativo,DataExpiracao,LimiteUsos,UsosAtuais")] Cupom cupom)
        {
            if (id != cupom.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (_context.Cupons.Any(c => c.Codigo == cupom.Codigo && c.Id != id))
                {
                    ModelState.AddModelError("Codigo", "Este código de cupom já existe.");
                    return View(cupom);
                }

                try
                {
                    _context.Update(cupom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CupomExists(cupom.Id))
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
            return View(cupom);
        }

        // GET: Admin/Cupons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cupom == null)
            {
                return NotFound();
            }

            return View(cupom);
        }

        // POST: Admin/Cupons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom != null)
            {
                _context.Cupons.Remove(cupom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CupomExists(int id)
        {
            return _context.Cupons.Any(e => e.Id == id);
        }
    }
}