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
        public IActionResult ChonLevel(string level)
        {
            // Lấy dữ liệu bài học theo cấp độ
            var selectedLevel = level ?? "N5";  // Mặc định là N5
            var levelId = GetLevelId(selectedLevel);

            var lessons = _context.Baihoc
                .Where(l => l.LevelId == levelId) // Lọc bài học theo LevelId
                .ToList();

            // Lấy thông tin cấp độ từ cơ sở dữ liệu, ví dụ:
            var levels = _context.Levels.ToList();

            ViewBag.Levels = levels;
            ViewData["Level"] = selectedLevel;

            return View(lessons);
        }
        private int GetLevelId(string level)
        {
            switch (level)
            {
                case "N5": return 1;  // Cấp độ N5 có LevelId = 1
                case "N4": return 2;  // Cấp độ N4 có LevelId = 2
                case "N3": return 3;  // Cấp độ N3 có LevelId = 3
                default: return 1;    // Mặc định là N5
            }

        }
        public async Task<IActionResult> Details(int id)
        {
            var level = await _context.Levels
                                      .Include(l => l.Lessons) 
                                      .ThenInclude(b => b.Level) 
                                      .FirstOrDefaultAsync(l => l.Id == id);

            if (level == null)
            {
                return NotFound();
            }

            return View(level);
        }

    }
}
