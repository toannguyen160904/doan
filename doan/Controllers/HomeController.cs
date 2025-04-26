using doan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using doan.Repository;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

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
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _vocabularyRepository = vocabularyRepository;
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
            var levels = await _context.Levels.Include(l => l.Lessons).ToListAsync();

            if (levels == null || !levels.Any())
            {
                Console.WriteLine("Không có dữ liệu Level nào!");
                return View(new List<Level>());
            }

            foreach (var level in levels)
            {
                Console.WriteLine($"Level: {level.Name} - Số bài học: {level.Lessons?.Count ?? 0}");
            }

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


        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
        [Authorize]
        public IActionResult Flashcards(int baiHocId)
        {
            var baiHoc = _context.Baihoc
                .Include(b => b.tuvung)
                .Include(b => b.nguphap)
                .FirstOrDefault(b => b.Id == baiHocId);

            if (baiHoc == null)
            {
                return NotFound();
            }

            var flashcards = new List<doan.Models.flashcards>();

            // Từ vựng
            foreach (var word in baiHoc.tuvung)
            {
                flashcards.Add(new doan.Models.flashcards
                {
                    Vocabulary = new Vocabulary
                    {
                        Tuvung = word.Tuvung,
                        PhatAm = word.PhatAm,
                        AmHan = word.AmHan,
                        HanTu = word.HanTu,
                        Nghia = word.Nghia
                    }
                });
            }

            // Ngữ pháp
            foreach (var grammar in baiHoc.nguphap)
            {
                flashcards.Add(new doan.Models.flashcards
                {
                    GrammarStructure = new GrammarStructure
                    {
                        CongThuc = grammar.CongThuc,
                        GiaiThich = grammar.GiaiThich,
                        CauViDu = grammar.CauViDu
                    }
                });
            }

            ViewData["BaiHocName"] = baiHoc.Name;
            return View(flashcards);
        }

    }
}
