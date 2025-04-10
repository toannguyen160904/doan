using doan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace doan.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lessons = await _context.Baihoc
                                .Include(l => l.Level)
                                .OrderBy(l => l.LevelId) // Sắp xếp tăng dần theo LevelId
                                .ToListAsync();

            return View(lessons);
        }

        public IActionResult Create()
        {
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Baihoc lesson)
        {
            if (ModelState.IsValid)
            {
                _context.Baihoc.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
            return View(lesson);
        }

        // GET: Admin/Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Baihoc.FindAsync(id);
            if (lesson == null) return NotFound();

            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
            return View(lesson);
        }

        // POST: Admin/Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Baihoc lesson)
        {
            if (id != lesson.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
            return View(lesson);
        }

        // GET: Admin/Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Baihoc.Include(l => l.Level).FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null) return NotFound();

            return View(lesson);
        }

        // POST: Admin/Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Baihoc.FindAsync(id);
            if (lesson != null)
            {
                _context.Baihoc.Remove(lesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Baihoc.Include(l => l.Level)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null) return NotFound();

            return View(lesson);
        }
    }
}
