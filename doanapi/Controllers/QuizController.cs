// File: doanapi/Controllers/QuizController.cs (ĐÃ VIẾT LẠI HOÀN CHỈNH)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using SharedModels.Models.DTO;
using SharedModels.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public QuizController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Endpoint này lấy dữ liệu của một bài quiz để học sinh làm bài
    [HttpGet("by-lesson/{baiHocId}")] // URL: /api/quiz/by-lesson/123
    public async Task<ActionResult<QuizForStudentViewModel>> GetQuizByLesson(int baiHocId)
    {
        // 1. THAY ĐỔI QUAN TRỌNG: Thêm .Include(q => q.Baihoc) để lấy được tên bài học
        var quiz = await _context.Quizzes
            .Include(q => q.Baihoc) // <-- PHẢI CÓ DÒNG NÀY
            .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
            .FirstOrDefaultAsync(q => q.BaihocId == baiHocId);

        if (quiz == null || quiz.Baihoc == null)
        {
            return NotFound("Quiz hoặc bài học liên quan không tồn tại.");
        }

        // 2. SỬ DỤNG VIEWMODEL: Tạo một ViewModel chuyên dụng để trả về cho client
        var quizViewModel = new QuizForStudentViewModel
        {
            Id = quiz.Id,
            // 3. SỬA LỖI: Lấy tên quiz từ tên của bài học liên quan
            TenQuiz = quiz.Baihoc.Name, // Giả sử thuộc tính tên là "TenBaiHoc"
            BaihocId = quiz.BaihocId,
            CauHois = quiz.CauHois.Select(ch => new QuestionViewModel
            {
                Id = ch.Id,
                NoiDung = ch.NoiDung,
                // Chuyển các câu trả lời sang AnswerViewModel để che đáp án đúng
                CauTraLois = ch.CauTraLois.Select(ctl => new AnswerViewModel
                {
                    Id = ctl.Id,
                    NoiDung = ctl.NoiDung
                }).ToList()
            }).ToList()
        };

        return Ok(quizViewModel);
    }

    // Endpoint này nhận bài làm và trả về kết quả
    [HttpPost("submit")] // URL: /api/quiz/submit
    public async Task<ActionResult<QuizResultViewModel>> SubmitQuiz([FromBody] SubmitQuizDto submission)
    {
        // Logic phần này của bạn đã khá tốt, giữ nguyên và chỉ cần đảm bảo ViewModel/DTO đúng
        var quiz = await _context.Quizzes
            .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
            .FirstOrDefaultAsync(q => q.Id == submission.QuizId);

        if (quiz == null)
        {
            return BadRequest("Quiz không tồn tại.");
        }

        int soCauDung = 0;
        var dapanDungDict = new Dictionary<int, int>();
        int tongSoCau = quiz.CauHois.Count;

        foreach (var cauHoi in quiz.CauHois)
        {
            var dapAnDung = cauHoi.CauTraLois.FirstOrDefault(ct => ct.IsCorrect);
            if (dapAnDung != null)
            {
                dapanDungDict[cauHoi.Id] = dapAnDung.Id;

                if (submission.UserAnswers.TryGetValue(cauHoi.Id, out int selectedAnswerId))
                {
                    if (selectedAnswerId == dapAnDung.Id)
                    {
                        soCauDung++;
                    }
                }
            }
        }

        var resultViewModel = new QuizResultViewModel
        {
            QuizId = quiz.Id,
            Score = soCauDung,
            TotalQuestions = tongSoCau,
            CorrectAnswers = dapanDungDict,
            UserAnswers = submission.UserAnswers
        };

        return Ok(resultViewModel);
    }
}


// =========================================================================
// CÁC LỚP VIEWMODEL VÀ DTO CẦN THIẾT
// Bạn hãy tạo các file tương ứng cho chúng trong project `SharedModels`
// =========================================================================

namespace SharedModels.Models.ViewModels
{
    // ViewModel để gửi dữ liệu bài quiz cho học sinh (đã che đáp án)
    public class QuizForStudentViewModel
    {
        public int Id { get; set; }
        public string TenQuiz { get; set; }
        public int BaihocId { get; set; }
        public List<QuestionViewModel> CauHois { get; set; }
    }

    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string NoiDung { get; set; }
        public List<AnswerViewModel> CauTraLois { get; set; }
    }

    // ViewModel cho câu trả lời, không chứa thuộc tính 'IsCorrect' để bảo mật
    public class AnswerViewModel
    {
        public int Id { get; set; }
        public string NoiDung { get; set; }
    }

    // ViewModel để trả về kết quả sau khi chấm điểm
    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public Dictionary<int, int> CorrectAnswers { get; set; }
        public Dictionary<int, int> UserAnswers { get; set; }
    }
}

namespace SharedModels.Models.DTO
{
    // DTO để nhận dữ liệu bài làm của người dùng
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }
        public Dictionary<int, int> UserAnswers { get; set; }
    }
}