using doan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using doan.Repository;
using System.Diagnostics;
<<<<<<< HEAD

using System.Collections.Generic;
using System.Linq;

using System.Collections.Generic;
using System.Linq;



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

        public async Task<IActionResult> Index()
        {
            var levels = await _context.Levels.ToListAsync();
            return View(levels);
        }

        public async Task<IActionResult> ChonLevel()
        {
            var levels = await _context.Levels.Include(l => l.Lessons).ToListAsync();
            if (levels == null || !levels.Any())
                return View(new List<Level>());

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
                    return RedirectToAction("Index1");
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

        // ✅ Xem từ vựng theo Level
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
                .ToListAsync();

            return View(tuVung); // Sử dụng Views/Home/TuVung.cshtml
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

        // ✅ Giao diện làm bài test đầu vào
        public async Task<IActionResult> Test()
        {
            var questions = await _context.TestQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => Guid.NewGuid())
                .Take(20)
                .ToListAsync();

            return View("~/Views/Home/Test.cshtml", questions);
        }

        // ✅ Xử lý nộp bài test đầu vào
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
    }
}
