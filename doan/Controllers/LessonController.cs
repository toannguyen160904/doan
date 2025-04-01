using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public class LessonController : Controller
{
    private readonly ApplicationDbContext _context;


    public LessonController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IActionResult Details(int id)
    {
        if (_context == null)
        {
            return Problem("Database context is not available.");
        }

        var lesson = _context.Baihoc
            .Include(l => l.tuvung)
            .Include(l => l.nguphap)
            .FirstOrDefault(l => l.Id == id);

        if (lesson == null)
        {
            return NotFound();
        }

        return View("~/Views/Level/Details.cshtml", lesson);
    }
    public IActionResult Create()
    {
        // Lấy danh sách cấp độ để hiển thị trong dropdown
        ViewBag.Levels = new SelectList(new List<string> { "N5", "N4", "N3" });
        return View();
    }

    // POST: Lesson/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Lấy LevelId từ cấp độ người dùng chọn
            int levelId = GetLevelId(model.LevelName);

            // Tạo bài học mới
            var lesson = new Baihoc
            {
                Name = model.Name,
                LevelId = levelId,  // Gán LevelId cho bài học
            };

            // Lưu bài học vào cơ sở dữ liệu
            _context.Add(lesson);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang danh sách bài học
            return RedirectToAction(nameof(Index));
        }

        // Nếu có lỗi, trả lại view
        ViewBag.Levels = new SelectList(new List<string> { "N5", "N4", "N3" });
        return View(model);
    }

    private int GetLevelId(string level)
    {
        switch (level)
        {
            case "N5":
                return 1;  // LevelId cho N5 là 1
            case "N4":
                return 2;  // LevelId cho N4 là 2
            case "N3":
                return 3;  // LevelId cho N3 là 3
            default:
                return 1;  // Mặc định là N5 nếu không chọn
        }
    }
}
