using doan.Models;
using doan.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using doan.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

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
            // Khai báo và lấy danh sách levels cùng với Lessons
            var levels = await _context.Levels
                .Include(l => l.Lessons)  // Bao gồm các bài học (Lessons)
                .ToListAsync();  // Chuyển đổi kết quả thành danh sách

            // Kiểm tra nếu không có dữ liệu Levels
            if (levels == null || !levels.Any())
            {
                _logger.LogWarning("Không có dữ liệu Level nào!");
                return View(new List<Level>());
            }

            // Trả về view với danh sách levels đã lấy từ cơ sở dữ liệu
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
                    return RedirectToAction("Index1");
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

        // ✅ Xem ngữ pháp theo Level
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

        [Authorize]
        public IActionResult Flashcards(int baiHocId)
        {
            var baiHoc = _context.Baihoc
                .Include(b => b.tuvung)
                .Include(b => b.nguphap)
                .FirstOrDefault(b => b.Id == baiHocId);

            if (baiHoc == null)
                return NotFound();

            var flashcards = new List<flashcards>();  // Đổi tên thành Flashcard, viết hoa chữ cái đầu

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
        // Action để hiển thị trang làm bài Quiz
        public IActionResult Quiz(int baiHocId)
        {
            // Lấy Quiz dựa trên bài học
            var quiz = _context.Quizzes
                .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
                .FirstOrDefault(q => q.BaihocId == baiHocId);

            if (quiz == null)
            {
                return NotFound("Quiz không tồn tại cho bài học này.");
            }

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
