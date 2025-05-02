using doan.Models;
using doan.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace doan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            IVocabularyRepository vocabularyRepository,
            ILogger<HomeController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _vocabularyRepository = vocabularyRepository;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var levels = await _context.Levels.ToListAsync();
            return View(levels);
        }

        [Authorize]
        public async Task<IActionResult> ChonLevel()
        {
            var levels = await _context.Levels
                .Include(l => l.Lessons)
                .ToListAsync();

            if (levels == null || !levels.Any())
            {
                _logger.LogWarning("Không có dữ liệu Level nào!");
                return View(new List<Level>());
            }

            return View(levels);
        }

        [HttpPost]
        [AllowAnonymous]
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

            ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View("Login");
        }

        public IActionResult MiniTest()
        {
            _logger.LogInformation("Người dùng đã vào trang Mini Test.");
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [Authorize]
        public IActionResult Flashcards(int baiHocId)
        {
            var baiHoc = _context.Baihoc
                .Include(b => b.tuvung)
                .Include(b => b.nguphap)
                .FirstOrDefault(b => b.Id == baiHocId);

            if (baiHoc == null)
                return NotFound();

            var flashcards = new List<flashcards>();

            // Thêm flashcard từ từ vựng
            flashcards.AddRange(baiHoc.tuvung.Select(word => new flashcards
            {
                Vocabulary = new Vocabulary
                {
                    Tuvung = word.Tuvung,
                    PhatAm = word.PhatAm,
                    AmHan = word.AmHan,
                    HanTu = word.HanTu,
                    Nghia = word.Nghia
                }
            }));

            // Thêm flashcard từ ngữ pháp
            flashcards.AddRange(baiHoc.nguphap.Select(grammar => new flashcards
            {
                GrammarStructure = new GrammarStructure
                {
                    CongThuc = grammar.CongThuc,
                    GiaiThich = grammar.GiaiThich,
                    CauViDu = grammar.CauViDu
                }
            }));

            ViewData["BaiHocName"] = baiHoc.Name;
            return View(flashcards);
        }
    }
}
