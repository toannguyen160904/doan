// File: doanapi/Controllers/TestController.cs (ĐÃ CHỈNH SỬA)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using SharedModels.Models.DTO;       // Thêm using cho DTO
using SharedModels.Models.ViewModels; // Thêm using cho ViewModel
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedModels.Models.ViewModels;

// 1. Thêm các attribute chuẩn
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase // 2. Kế thừa từ ControllerBase
{
    private readonly ApplicationDbContext _context;
    private const int NumberOfTestQuestions = 20;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 3. Chuyển đổi Action 'Start' và 'DoTest' thành một endpoint duy nhất
    // Endpoint này sẽ bắt đầu một bài test mới bằng cách tạo và trả về một bộ câu hỏi ngẫu nhiên.
    [HttpGet("start")] // URL: /api/test/start
    public async Task<ActionResult<IEnumerable<TestQuestionViewModel>>> StartTest()
    {
        // Lấy câu hỏi ngẫu nhiên từ DB
        var questions = await _context.TestQuestions
            .Where(q => q.IsActive)
            .OrderBy(q => Guid.NewGuid())
            .Take(NumberOfTestQuestions)
            .ToListAsync();

        if (!questions.Any())
        {
            // Trả về lỗi nếu không có câu hỏi nào
            return NotFound("Không có câu hỏi nào trong hệ thống để bắt đầu bài test.");
        }

        // 4. Chuyển đổi sang ViewModel để che đáp án đúng
        // Client chỉ nhận được câu hỏi và các lựa chọn
        var questionViewModels = questions.Select(q => new TestQuestionViewModel
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Choices = q.Choices
        }).ToList();

        return Ok(questionViewModels);
    }

    // 5. Chuyển đổi Action 'Submit' để nhận bài làm và trả về kết quả
    [HttpPost("submit")] // URL: /api/test/submit
    public async Task<ActionResult<TestResultViewModel>> SubmitTest([FromBody] List<TestAnswerInput> userAnswers)
    {
        if (userAnswers == null || !userAnswers.Any())
        {
            return BadRequest("Không có câu trả lời nào được gửi lên.");
        }

        // Tách logic chấm điểm ra một phương thức riêng
        int correctAnswersCount = await CalculateScoreAsync(userAnswers);

        // Tách logic xác định trình độ
        string suggestedLevel = DetermineLevel(correctAnswersCount);

        // 6. Tạo một ViewModel để chứa kết quả trả về
        var resultViewModel = new TestResultViewModel
        {
            CorrectAnswers = correctAnswersCount,
            TotalQuestions = userAnswers.Count, // Tổng số câu là số câu người dùng đã trả lời
            SuggestedLevel = suggestedLevel
        };

        return Ok(resultViewModel);
    }


    // Logic chấm điểm giữ nguyên, nhưng giờ nó là một phần của API
    private async Task<int> CalculateScoreAsync(List<TestAnswerInput> answers)
    {
        int choiceIndex = -1;
        if (answers == null || !answers.Any()) return 0;

        int correctCount = 0;
        var questionIds = answers.Select(a => a.QuestionId).ToList();

        var questionsInTest = await _context.TestQuestions
                                .Where(q => questionIds.Contains(q.Id))
                                            .ToDictionaryAsync(q => q.Id);

        foreach (var ans in answers)
        {
            // Gộp tất cả các điều kiện kiểm tra vào một khối if duy nhất
            if (questionsInTest.TryGetValue(ans.QuestionId, out var question) && // Điều kiện 1
            int.TryParse(ans.SelectedAnswer, out  choiceIndex) &&       // Điều kiện 2
            choiceIndex >= 0 &&                                             // Điều kiện 3
            choiceIndex < question.Choices.Count)                           // Điều kiện 4
            {
                // ... code bên trong chỉ chạy nếu TẤT CẢ 4 điều kiện trên đều đúng ...
                string selectedChoiceText = question.Choices[choiceIndex];
                if (question.CorrectAnswer == selectedChoiceText)
                {
                    correctCount++;
                }
            }
        }

        return correctCount;
    }

    // Logic xác định trình độ giữ nguyên
    private string DetermineLevel(int correctCount)
    {
        return correctCount switch
        {
            >= 15 => "N3",
            >= 8 => "N4",
            _ => "N5"
        };
    }

}