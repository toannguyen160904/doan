using SharedModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using SharedModels.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace doan.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _apiBaseUrl;

        public HomeController(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<HomeController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new Exception("HttpContext chưa được khởi tạo.");

            var uri = new Uri(_apiBaseUrl);
            foreach (var cookie in httpContext.Request.Cookies)
            {
                handler.CookieContainer.Add(uri, new System.Net.Cookie(cookie.Key, cookie.Value));
            }

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = uri
            };
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
                    return RedirectToAction("~/Lessons/Index");
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

        public async Task<IActionResult> TuVung(int? levelId)
        {
            if (levelId == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var learnedVocabIds = new List<int>();

            if (user != null)
            {
                learnedVocabIds = await _context.UserVocabularyProgresses
                    .Where(p => p.UserId == user.Id && p.IsLearned)
                    .Select(p => p.VocabularyId)
                    .ToListAsync();
            }

            var tuVung = await _context.tuvung
                .Include(v => v.Lesson)
                .Where(v => v.Lesson.LevelId == levelId)
                .Select(v => new VocabularyWithStatus
                {
                    Id = v.Id,
                    Word = v.Tuvung,
                    Meaning = v.Nghia,
                    LessonName = v.Lesson.Name,
                    IsLearned = learnedVocabIds.Contains(v.Id)
                })
                .ToListAsync();

            return View("~/Views/Home/TuVung.cshtml", tuVung);
        }

        public async Task<IActionResult> NguPhap(int? levelId)
        {
            if (levelId == null) return NotFound();

            var nguPhap = await _context.nguphap
                .Include(g => g.Lesson)
                .Where(g => g.Lesson.LevelId == levelId)
                .ToListAsync();

            return View(nguPhap);
        }

        [Authorize]
        public async Task<IActionResult> Test()
        {
            var questions = await _context.TestQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => Guid.NewGuid())
                .Take(20)
                .ToListAsync();

            return View("~/Views/Home/Test.cshtml", questions);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(List<TestAnswerInput> answers)
        {
            int correct = 0;
            foreach (var ans in answers)
            {
                var question = await _context.TestQuestions.FindAsync(ans.QuestionId);
                if (question != null && question.CorrectAnswer == ans.SelectedAnswer)
                    correct++;
            }

            string level = "N5";
            if (correct >= 15) level = "N3";
            else if (correct >= 8) level = "N4";

            var result = new TestResult
            {
                CorrectAnswers = correct,
                TotalScore = correct,
                SuggestedLevel = level
            };

            return View("~/Views/Home/TestResult.cshtml", result);
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
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Quiz(int baiHocId)
        {
            var quiz = _context.Quizzes
                .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
                .FirstOrDefault(q => q.BaihocId == baiHocId);

            if (quiz == null)
                return NotFound("Quiz không tồn tại cho bài học này.");

            var baiHoc = _context.Baihoc.FirstOrDefault(b => b.Id == baiHocId);
            ViewData["BaiHocName"] = baiHoc.Name;

            return View(quiz);
        }

        public async Task<IActionResult> DanhSachQuiz()
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
    }
}
