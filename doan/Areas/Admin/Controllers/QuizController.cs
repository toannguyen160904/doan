using doan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuizController(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // GET: Admin/Quiz/Index
    public async Task<IActionResult> Index()
    {
        // Lấy danh sách bài học cùng với các bài quiz liên kết
        var baiHocs = await _context.Baihoc
            .Include(b => b.Quiz) // Bao gồm thông tin bài quiz
            .ToListAsync();

        return View(baiHocs);
    }
    // GET: Admin/Quiz/CreateCauHoi
    public IActionResult CreateQuiz(int baiHocId)
    {
        // Truyền thông tin bài học vào ViewData để hiển thị nếu cần
        ViewData["BaiHocId"] = baiHocId;
        ViewData["BaiHocName"] = _context.Baihoc
            .Where(b => b.Id == baiHocId)
            .Select(b => b.Name)
            .FirstOrDefault();

        // Trả về view CreateCauHoi
        return View("CreateQuiz");
    }

    // POST: Admin/Quiz/CreateQuiz
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateQuiz(int baiHocId, List<CauHoi> questions)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra xem bài quiz đã tồn tại chưa
            var quiz = _context.Quizzes.FirstOrDefault(q => q.BaihocId == baiHocId);
            if (quiz == null)
            {
                quiz = new Quiz
                {
                    BaihocId = baiHocId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Quizzes.Add(quiz);
                _context.SaveChanges();
            }

            // Lưu danh sách câu hỏi và câu trả lời
            foreach (var question in questions)
            {
                var cauHoi = new CauHoi
                {
                    NoiDung = question.NoiDung,
                    QuizId = quiz.Id
                };
                _context.CauHois.Add(cauHoi);
                _context.SaveChanges();

                // Lưu các câu trả lời cho câu hỏi
                foreach (var answer in question.CauTraLois)
                {
                    var cauTraLoi = new CauTraLoi
                    {
                        NoiDung = answer.NoiDung,
                        IsCorrect = answer.IsCorrect,
                        CauHoiId = cauHoi.Id
                    };
                    _context.CauTraLois.Add(cauTraLoi);
                }
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Quiz");
        }

        // Nếu có lỗi, trả về view với dữ liệu hiện tại
        ViewData["BaiHocId"] = baiHocId;
        return View();
    }


    public IActionResult CreateCauHoi(CauHoi cauHoi)
    {
        if (ModelState.IsValid)
        {
            _context.CauHois.Add(cauHoi);
            _context.SaveChanges();
            return RedirectToAction("CreateQuiz", new { baiHocId = cauHoi.QuizId });
        }
        var quiz = _context.Quizzes
            .Include(q => q.CauHois)
            .FirstOrDefault(q => q.Id == cauHoi.QuizId);

        ViewData["BaiHocName"] = _context.Baihoc
            .Where(b => b.Id == quiz.BaihocId)
            .Select(b => b.Name)
            .FirstOrDefault();
        return View("Index",quiz); //"CreateQuiz", quiz
    }

    [HttpPost]//Câu Trả lời đúng
    [ValidateAntiForgeryToken]
    public IActionResult CapNhatDapAnDung(int cauTraLoiId, bool isCorrect)
    {
        var cauTraLoi = _context.CauTraLois.FirstOrDefault(c => c.Id == cauTraLoiId);
        if (cauTraLoi == null)
            return NotFound();

        cauTraLoi.IsCorrect = isCorrect;
        _context.SaveChanges();

        return Ok(new { success = true });
    }

}
