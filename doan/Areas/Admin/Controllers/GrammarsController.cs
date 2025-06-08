using doan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace doan.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class GrammarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GrammarsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var grammar = await _context.nguphap
                .Include(v => v.Lesson)
                .ThenInclude(l => l.Level)
                .OrderBy(x => x.Lesson.LevelId)
                .ToListAsync();

            ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name");
            return View(grammar);
        }
        [HttpGet]
        public async Task<IActionResult> _GrammarListPartial(int? levelId, int? lessonId)
        {
            // Bắt đầu với một query có thể lọc được
            var query = _context.nguphap
                                .Include(g => g.Lesson)
                                .ThenInclude(l => l.Level)
                                .AsQueryable();

            // Lọc theo Bài học (ưu tiên cao nhất)
            if (lessonId.HasValue && lessonId > 0)
            {
                query = query.Where(g => g.LessonId == lessonId.Value);
            }
            // Nếu không có bài học, lọc theo Cấp độ
            else if (levelId.HasValue && levelId > 0)
            {
                query = query.Where(g => g.Lesson.LevelId == levelId.Value);
            }

            // Thực thi query và sắp xếp
            var filteredGrammar = await query.OrderBy(g => g.Lesson.LevelId).ThenBy(g => g.LessonId).ToListAsync();

            // Trả về Partial View với dữ liệu đã được lọc
            return PartialView("_GrammarListPartial", filteredGrammar);
        }
        [HttpGet("/Admin/Grammars/GetByLevel/{levelId}")]
        public IActionResult GetByLevel(int levelId)
        {
            var lessons = _context.Baihoc
                .Where(b => b.LevelId == levelId && !b.IsDeleted)
                .Select(b => new { id = b.Id, name = b.Title })
                .ToList();

            return Ok(lessons);
        }
        //[HttpGet("/Admin/Grammars/GetByLevel/{levelId}")]
        //public IActionResult GetByLevel(int levelId)
        //{
        //    var lessons = _context.Baihoc
        //        .Where(b => b.LevelId == levelId)
        //        .Select(b => new { id = b.Id, name = b.Title })
        //        .ToList();

        //    return Ok(lessons);
        //}

        public IActionResult Create()
        {
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            ViewBag.LessonId = new SelectList(_context.Baihoc, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GrammarStructure model)
        {
            if (ModelState.IsValid)
            {
                _context.nguphap.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            ViewBag.LessonId = new SelectList(_context.Baihoc, "Id", "Title", model.LessonId);
            return View(model);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var grammar = await _context.nguphap.FindAsync(id);
            if (grammar == null) return NotFound();

            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            ViewBag.LessonId = new SelectList(_context.Baihoc, "Id", "Title", grammar.LessonId);

            return View(grammar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GrammarStructure model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");

            ViewBag.LessonId = new SelectList(_context.Baihoc, "Id", "Title", model.LessonId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var grammar = await _context.nguphap
                .Include(g => g.Lesson)
                .ThenInclude(l => l.Level)  // Lấy thêm cấp độ
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grammar == null) return NotFound();

            return View(grammar);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grammar = await _context.nguphap.FindAsync(id);
            if (grammar != null)
            {
                _context.nguphap.Remove(grammar);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var grammar = await _context.nguphap
                .Include(g => g.Lesson)
                .ThenInclude(l => l.Level) // load thêm cấp độ
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grammar == null) return NotFound();

            return View(grammar);
        }

    }
}
