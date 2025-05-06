using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan.Models;
using System.Linq;
using System.Threading.Tasks;

namespace doan.Controllers
{
    public class LevelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LevelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách bài học theo cấp độ
        public IActionResult ChonLevel(string level)
        {
            var selectedLevel = level ?? "N5"; // Mặc định là N5 nếu không chọn
            var levelId = GetLevelId(selectedLevel);

            // Lọc bài học theo cấp độ
            var lessons = _context.Baihoc
                .Where(l => l.LevelId == levelId)
                .ToList();

            // Lấy danh sách tất cả cấp độ
            ViewBag.Levels = _context.Levels.ToList();
            ViewData["Level"] = selectedLevel;

            return View(lessons);
        }

        // Trả về thông tin chi tiết của một cấp độ
        public async Task<IActionResult> Details(int id)
        {
            var level = await _context.Levels
                .Include(l => l.Lessons)
                .ThenInclude(b => b.Level)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (level == null)
                return NotFound();

            return View(level);
        }

        // Hàm ánh xạ tên cấp độ thành ID (giả định ID cố định trong DB)
        private int GetLevelId(string level) => level switch
        {
            "N5" => 1,
            "N4" => 2,
            "N3" => 3,
            _ => 1 // Mặc định là N5
        };
    }
}
