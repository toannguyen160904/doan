using SharedModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

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

              return View(await _context.Baihoc.ToListAsync());
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

            if (await _context.Baihoc.AnyAsync(b => b.Name == lesson.Name && b.LevelId == lesson.LevelId))
            {
                ModelState.AddModelError("Name", "Tên bài học đã tồn tại.");
                ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name", lesson.LevelId);
                return View(lesson);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ.");
                ViewBag.Levels = new SelectList(await _context.Levels.ToListAsync(), "Id", "Name", lesson.LevelId);
                return View(lesson);
            }

            try
            {
                // Lấy toàn bộ Order đã dùng (kể cả bản đã xóa mềm)
                var usedOrders = await _context.Baihoc
                    .IgnoreQueryFilters()
                    .Where(b => b.LevelId == lesson.LevelId)
                    .Select(b => b.Order)
                    .ToListAsync();

                int nextOrder;

                if (lesson.Order > 0)
                {
                    // Nếu user nhập sẵn Order và bị trùng thì tăng dần
                    nextOrder = lesson.Order;
                    while (usedOrders.Contains(nextOrder)) nextOrder++;
                }
                else
                {
                    // Nếu không nhập Order hoặc <= 0 thì chọn số nhỏ nhất chưa dùng
                    nextOrder = 1;
                    var usedSet = new HashSet<int>(usedOrders);
                    while (usedSet.Contains(nextOrder)) nextOrder++;
                }

                lesson.Order = nextOrder;
                lesson.CreatedAt = DateTime.UtcNow;

                _context.Baihoc.Add(lesson);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bài học đã được tạo thành công.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài học");
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
            bool tentrung = await _context.Baihoc.AnyAsync(b =>b.Name == lesson.Name&& b.Id !=lesson.Id);
            if (tentrung)
            {
                ModelState.AddModelError("Name", "Tên bài học đã tồn tại.");
                ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name", lesson.LevelId);
                return View(lesson);
            }

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
               lesson.IsDeleted = true;
                _context.Baihoc.Update(lesson);
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
        public async Task<IActionResult> IndexThungrac()
        {
            var deletedLessons = await _context.Baihoc
                .IgnoreQueryFilters()
                .Where(b => b.IsDeleted)
                .Include(b => b.Level)
                .ToListAsync();

            return View(deletedLessons);
        }
        public async Task<IActionResult> Restore(int id)
        {
            var lesson = await _context.Baihoc
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (lesson != null)
            {
                lesson.IsDeleted = false;
                _context.Update(lesson);
                await _context.SaveChangesAsync();
            }

            // Sau khi khôi phục, điều hướng về trang Trash để làm mới danh sách
            return RedirectToAction("IndexThungrac");
        }
        private async Task<List<Baihoc>> GetDeletedLessons()
        {
            return await _context.Baihoc
                .IgnoreQueryFilters()
                .Where(b => b.IsDeleted)
                .Include(b => b.Level)
                .ToListAsync();
        }
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var lesson = await _context.Baihoc
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (lesson != null)
            {
                _context.Baihoc.Remove(lesson); // Xóa khỏi DB
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("IndexThungrac"); // Quay lại trang thùng rác
        }
    }
}
