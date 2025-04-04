using doan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace doan.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class VocabulariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VocabulariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vocabularies = await _context.tuvung.Include(v => v.Lesson).ToListAsync();
            return View(vocabularies);
        }

        public IActionResult Create()
        {
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            ViewBag.Lessons = new SelectList(_context.Baihoc, "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vocabulary vocab)
        {
            if (ModelState.IsValid)
            {
                _context.tuvung.Add(vocab);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LessonId"] = new SelectList(_context.Baihoc, "Id", "Name", vocab.LessonId);
            return View(vocab);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.tuvung.FindAsync(id);
            if (vocab == null) return NotFound();
            ViewData["LessonId"] = new SelectList(_context.Baihoc, "Id", "Name", vocab.LessonId);
            return View(vocab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vocabulary vocab)
        {
            if (id != vocab.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(vocab);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LessonId"] = new SelectList(_context.Baihoc, "Id", "Name", vocab.LessonId);
            return View(vocab);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.tuvung.Include(v => v.Lesson).FirstOrDefaultAsync(v => v.Id == id);
            if (vocab == null) return NotFound();
            return View(vocab);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vocab = await _context.tuvung.FindAsync(id);
            if (vocab != null)
            {
                _context.tuvung.Remove(vocab);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
