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

        public async Task<IActionResult> Index()
        {
            var lessons = await _context.Baihoc.Include(l => l.Level).ToListAsync();
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
            _logger.LogInformation($"[POST] Tạo bài học: {lesson.Name}, LevelId: {lesson.LevelId}");

            // Loại bỏ validation cho "Level" nếu nó không phải là input từ form
            ModelState.Remove("Level");

            // Kiểm tra ModelState hợp lệ hay không
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ. Các lỗi:");

                // Ghi log lỗi của ModelState
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        _logger.LogWarning($"Key: {error.Key}, Error: {err.ErrorMessage}");
                    }
                }

                // Load lại danh sách Levels để dropdown không bị rỗng
                ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name", lesson.LevelId);

                return View(lesson); // Trả lại view với model lỗi để hiển thị thông báo
            }

            try
            {
                // Thêm bài học mới vào database
                _context.Baihoc.Add(lesson);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bài học đã được tạo thành công.");

                return RedirectToAction(nameof(Index)); // Quay về danh sách bài học
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tạo bài học: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.");

                // Load lại danh sách Levels trước khi trả về view
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
