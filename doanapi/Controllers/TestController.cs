// File: doanapi/Controllers/TestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;                   // ApplicationDbContext
using SharedModels.Models;            // Entities (TestQuestion, ...)
using SharedModels.Models.ViewModels; // ViewModels trả ra API
using DTO = SharedModels.Models.DTO;  // <-- alias để dùng DTO.TestAnswerInput

namespace doanapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const int NumberOfTestQuestions = 20;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/test/start
        [HttpGet("start")]
        public async Task<ActionResult<IEnumerable<TestQuestionViewModel>>> StartTest()
        {
            var questions = await _context.TestQuestions
                .Where(q => q.IsActive)
                .OrderBy(_ => EF.Functions.Random())
                .Take(NumberOfTestQuestions)
                .ToListAsync();

            if (questions.Count == 0)
                return NotFound("Không có câu hỏi nào trong hệ thống để bắt đầu bài test.");

            var vm = questions.Select(q => new TestQuestionViewModel
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Choices = q.Choices
            }).ToList();

            return Ok(vm);
        }

        // POST /api/test/submit
        [HttpPost("submit")]
        public async Task<ActionResult<TestResultViewModel>> SubmitTest([FromBody] List<DTO.TestAnswerInput> userAnswers) // <-- dùng alias
        {
            if (userAnswers == null || userAnswers.Count == 0)
                return BadRequest("Không có câu trả lời nào được gửi lên.");

            var correct = await CalculateScoreAsync(userAnswers);
            var level = DetermineLevel(correct);

            var result = new TestResultViewModel
            {
                CorrectAnswers = correct,
                TotalQuestions = userAnswers.Count,
                SuggestedLevel = level
            };

            return Ok(result);
        }

        private async Task<int> CalculateScoreAsync(List<DTO.TestAnswerInput> answers) // <-- dùng alias
        {
            if (answers == null || answers.Count == 0) return 0;

            int correctCount = 0;
            var ids = answers.Select(a => a.QuestionId).ToList();

            var map = await _context.TestQuestions
                .Where(q => ids.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            foreach (var ans in answers)
            {
                if (map.TryGetValue(ans.QuestionId, out var q)
                    && int.TryParse(ans.SelectedAnswer, out var idx)
                    && idx >= 0 && idx < q.Choices.Count)
                {
                    var selected = q.Choices[idx];
                    if (q.CorrectAnswer == selected) correctCount++;
                }
            }
            return correctCount;
        }

        private string DetermineLevel(int correctCount) => correctCount switch
        {
            >= 15 => "N3",
            >= 8 => "N4",
            _ => "N5"
        };
    }
}
