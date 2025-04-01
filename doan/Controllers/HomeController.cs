using System.Diagnostics;
using doan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using doan.Repository;

namespace doan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
             ApplicationDbContext context,
             IVocabularyRepository vocabularyRepository,
            ILogger<HomeController> logger,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _vocabularyRepository = vocabularyRepository;
        }

        public async Task<IActionResult> Index()
        {

            var levels = await _context.Levels.ToListAsync();
            return View(levels);
        }
        public async Task<IActionResult> ChonLevel()
        {
            // Lấy danh sách cấp độ (Levels) và bao gồm danh sách bài học (Lessons) liên quan đến mỗi cấp độ
            var levels = await _context.Levels.Include(l => l.Lessons).ToListAsync();

            // Kiểm tra nếu không có cấp độ nào trong database
            if (levels == null || !levels.Any())
            {
                Console.WriteLine("Không có dữ liệu Level nào!");
                return View(new List<Level>()); // Tránh truyền null vào View
            }

            // Hiển thị thông tin về từng cấp độ và số lượng bài học liên quan
            foreach (var level in levels)
            {
                Console.WriteLine($"Level: {level.Name} - Số bài học: {level.Lessons?.Count ?? 0}");
            }

            // Trả về View và truyền danh sách cấp độ có kèm bài học vào View
            return View(levels);
        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index1");
                }
            }
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View("Login");
        }

        public IActionResult MiniTest()
        {
            _logger.LogInformation("Người dùng đã vào trang Mini Test.");
            return View();
        }
  
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
