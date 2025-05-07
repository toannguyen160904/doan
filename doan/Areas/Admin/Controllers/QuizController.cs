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

    // GET: Admin/Quiz/Edit/5
    public IActionResult Edit(int id)
    {
        var quiz = _context.Quizzes
            .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
            .FirstOrDefault(q => q.Id == id);
        if (quiz == null)
        {
            return NotFound();
        }
        ViewData["BaiHocName"] = _context.Baihoc.Where(b => b.Id == quiz.BaihocId).Select(b => b.Name).FirstOrDefault();
        return View(quiz);
    }

    // POST: Admin/Quiz/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Quiz quiz)
    {
        if (id != quiz.Id)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            var existingQuiz = _context.Quizzes
                .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
                .FirstOrDefault(q => q.Id == id);
            if (existingQuiz == null)
            {
                return NotFound();
            }
            // Update quiz properties if needed (add more fields as required)
            existingQuiz.BaihocId = quiz.BaihocId;
            // Update questions and answers (now supports adding new ones)
            foreach (var question in quiz.CauHois)
            {
                var existingQuestion = existingQuiz.CauHois.FirstOrDefault(q => q.Id == question.Id);
                if (existingQuestion != null)
                {
                    // Update existing question
                    existingQuestion.NoiDung = question.NoiDung;
                    foreach (var answer in question.CauTraLois)
                    {
                        var existingAnswer = existingQuestion.CauTraLois.FirstOrDefault(a => a.Id == answer.Id);
                        if (existingAnswer != null)
                        {
                            existingAnswer.NoiDung = answer.NoiDung;
                            existingAnswer.IsCorrect = answer.IsCorrect;
                        }
                        else if (answer.Id == 0)
                        {
                            var newAnswer = new CauTraLoi
                            {
                                NoiDung = answer.NoiDung,
                                IsCorrect = answer.IsCorrect,
                                CauHoiId = existingQuestion.Id
                            };
                            _context.CauTraLois.Add(newAnswer);
                        }
                    }
                }
                else if (question.Id == 0)
                {
                    // Add new question
                    question.QuizId = existingQuiz.Id;
                    _context.CauHois.Add(question);
                    _context.SaveChanges(); // Save to get the new question Id
                    // Add new answers for this new question
                    foreach (var answer in question.CauTraLois)
                    {
                        answer.CauHoiId = question.Id;
                        _context.CauTraLois.Add(answer);
                    }
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        ViewData["BaiHocName"] = _context.Baihoc.Where(b => b.Id == quiz.BaihocId).Select(b => b.Name).FirstOrDefault();
        return View(quiz);
    }

    // GET: Admin/Quiz/Delete/5
    public IActionResult Delete(int id)
    {
        var quiz = _context.Quizzes
            .Include(q => q.CauHois)
            .ThenInclude(ch => ch.CauTraLois)
            .FirstOrDefault(q => q.Id == id);
        if (quiz == null)
        {
            return NotFound();
        }
        ViewData["BaiHocName"] = _context.Baihoc.Where(b => b.Id == quiz.BaihocId).Select(b => b.Name).FirstOrDefault();
        return View(quiz);
    }

    // POST: Admin/Quiz/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var quiz = _context.Quizzes
                .Include(q => q.CauHois)
                    .ThenInclude(ch => ch.CauTraLois)
                .FirstOrDefault(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            // First delete all answers
            foreach (var question in quiz.CauHois)
            {
                _context.CauTraLois.RemoveRange(question.CauTraLois);
            }
            _context.SaveChanges();

            // Then delete all questions
            _context.CauHois.RemoveRange(quiz.CauHois);
            _context.SaveChanges();

            // Finally delete the quiz
            _context.Quizzes.Remove(quiz);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Quiz deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the error here if you have logging configured
            TempData["ErrorMessage"] = "An error occurred while deleting the quiz.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteQuestion([FromBody] DeleteIdModel model)
    {
        var question = _context.CauHois
            .Include(q => q.CauTraLois)
            .FirstOrDefault(q => q.Id == model.questionId);

        if (question == null)
            return Json(new { success = false, message = "Không tìm thấy câu hỏi." });

        _context.CauTraLois.RemoveRange(question.CauTraLois);
        _context.CauHois.Remove(question);
        _context.SaveChanges();

        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAnswer([FromBody] DeleteIdModel model)
    {
        var answer = _context.CauTraLois.FirstOrDefault(a => a.Id == model.answerId);
        if (answer == null)
            return Json(new { success = false, message = "Không tìm thấy câu trả lời." });

        _context.CauTraLois.Remove(answer);
        _context.SaveChanges();

        return Json(new { success = true });
    }

    public class DeleteIdModel
    {
        public int questionId { get; set; }
        public int answerId { get; set; }
    }

}
