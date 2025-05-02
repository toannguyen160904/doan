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
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(ApplicationDbContext context, ILogger<LessonsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Lessons
        public async Task<IActionResult> Index()
        {
            var lessons = await _context.Baihoc
                .Include(l => l.Level)
                .OrderBy(l => l.LevelId)
                .ToListAsync();

            return View(lessons);
        }

        // GET: Admin/Lessons/Create
        public IActionResult Create()
        {
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            return View();
        }

        // POST: Admin/Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Baihoc lesson)
        {
            _logger.LogInformation($"[POST] Tạo bài học: {lesson.Name}, LevelId: {lesson.LevelId}");

            ModelState.Remove("Level");
            ModelState.Remove("Flashcards");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ.");
                ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name", lesson.LevelId);
                return View(lesson);
            }

            try
            {
                _context.Baihoc.Add(lesson);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Bài học đã được tạo thành công.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tạo bài học: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.");
                ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name", lesson.LevelId);
                return View(lesson);
            }
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

            ModelState.Remove("Flashcards");
            ModelState.Remove("Level");

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

            var lesson = await _context.Baihoc.Include(l => l.Level)
                                               .Include(l => l.Flashcards)
                                               .FirstOrDefaultAsync(m => m.Id == id);
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
