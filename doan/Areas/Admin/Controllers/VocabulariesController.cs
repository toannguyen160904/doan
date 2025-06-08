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
            var vocabularies = await _context.tuvung
                .Include(v => v.Lesson)
                .ThenInclude(l => l.Level)
                .OrderBy(v => v.Lesson.LevelId)
                .ThenBy(v => v.LessonId)
                .ToListAsync();

            ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name");
            return View(vocabularies);
        }
        [HttpGet]
        public async Task<IActionResult> _VocabularyListPartial(int? levelId, int? lessonId)
        {
            var query = _context.tuvung
                .Include(v => v.Lesson)
                .ThenInclude(l => l.Level)
                .AsQueryable();

            if (lessonId.HasValue && lessonId > 0)
            {
                query = query.Where(v => v.LessonId == lessonId.Value);
            }
            else if (levelId.HasValue && levelId > 0)
            {
                query = query.Where(v => v.Lesson.LevelId == levelId.Value);
            }

            var result = await query
                .OrderBy(v => v.Lesson.LevelId)
                .ThenBy(v => v.LessonId)
                .ToListAsync();

            return PartialView("_VocabularyListPartial", result);
        }
        [HttpGet("/Admin/Vocabularies/GetLessonsByLevel/{levelId}")]
        public IActionResult GetLessonsByLevel(int levelId)
        {
            var lessons = _context.Baihoc
                .Where(b => b.LevelId == levelId && !b.IsDeleted)
                .Select(b => new { id = b.Id, name = b.Title })
                .ToList();

            return Ok(lessons);
        }

        public IActionResult Create()
        {
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vocabulary vocab)
        {
            Console.WriteLine("📥 FORM SUBMITTED ✅");

            Console.WriteLine($"📌 LessonId: {vocab.LessonId}");
            Console.WriteLine($"📌 Tuvung: {vocab.Tuvung}");
            Console.WriteLine($"📌 Nghia: {vocab.Nghia}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState không hợp lệ!");
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"❗ Lỗi tại {entry.Key}: {error.ErrorMessage}");
                    }
                }
                ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
                return View(vocab);
            }

            // Lưu thử
            _context.tuvung.Add(vocab);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Lưu thành công!");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vocab = await _context.tuvung.Include(v => v.Lesson)
                                             .FirstOrDefaultAsync(m => m.Id == id);

            if (vocab == null) return NotFound();

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
