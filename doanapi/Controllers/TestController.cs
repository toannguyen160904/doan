// File: doanapi/Controllers/TestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;                   // ApplicationDbContext
using SharedModels.Models;            // Entities (TestQuestion, ...)
using SharedModels.Models.ViewModels; // ViewModels trả ra API
using DTO = SharedModels.Models.DTO;  // alias DTO
using System.Text.Json;

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
                // ✅ Build từ ChoicesJson và chuyển sang List<string>
                Choices = string.IsNullOrWhiteSpace(q.ChoicesJson)
                    ? new List<string>()
                    : (JsonSerializer.Deserialize<string[]>(q.ChoicesJson)?.ToList()
                       ?? new List<string>())
            }).ToList();

            return Ok(vm);
        }

        // POST /api/test/submit
        [HttpPost("submit")]
        public async Task<ActionResult<TestResultViewModel>> SubmitTest([FromBody] List<DTO.TestAnswerInput> userAnswers)
        {
            if (userAnswers == null || userAnswers.Count == 0)
                return BadRequest("Không có câu trả lời nào được gửi lên.");

            var result = await CalculateScoreAsync(userAnswers);
            return Ok(result);
        }

        // Hỗ trợ SelectedAnswer là "1" (index) hoặc "A. ..."(text)
        private static int? ResolveSelectedIndex(string? selectedAnswer, string[] choices)
        {
            if (string.IsNullOrWhiteSpace(selectedAnswer)) return null;

            // 1) Client gửi chỉ số dạng chuỗi: "0", "1", ...
            if (int.TryParse(selectedAnswer, out var idx))
                return (idx >= 0 && idx < choices.Length) ? idx : (int?)null;

            // 2) Client gửi text: so khớp không phân biệt hoa thường, trim
            var i = Array.FindIndex(
                choices,
                c => string.Equals(c?.Trim(), selectedAnswer.Trim(), StringComparison.OrdinalIgnoreCase)
            );
            return i >= 0 ? i : (int?)null;
        }

        // ✅ Tính điểm + gợi ý level theo level của câu (1 câu -> lấy level của câu đó)
        private async Task<TestResultViewModel> CalculateScoreAsync(List<DTO.TestAnswerInput> answers)
        {
            int total = answers.Count;
            int correct = 0;

            // Theo dõi độ chính xác theo từng level
            var stats = new Dictionary<string, (int total, int correct)>(StringComparer.OrdinalIgnoreCase);

            foreach (var ans in answers)
            {
                var q = await _context.TestQuestions.FindAsync(ans.QuestionId);
                if (q == null) continue;

                var choices = string.IsNullOrWhiteSpace(q.ChoicesJson)
                    ? Array.Empty<string>()
                    : (JsonSerializer.Deserialize<string[]>(q.ChoicesJson) ?? Array.Empty<string>());


                var idx = ResolveSelectedIndex(ans.SelectedIndex, choices);
                var correctIdx = NormalizeCorrectIndex(q.CorrectAnswer, choices.ToList());

                bool isCorrect = (idx != null && correctIdx != null && idx == correctIdx);
                var lvl = q.Level ?? "N5";
                if (!stats.ContainsKey(lvl))
                    stats[lvl] = (0, 0);

                var s = stats[lvl];
                s.total += 1;
                if (isCorrect) { s.correct += 1; correct++; }
                stats[lvl] = s;
            }

            // --- Chọn suggestedLevel ---
            string suggestedLevel;
            if (total == 1)
            {
                // 1 câu => gợi ý theo level của chính câu đó
                suggestedLevel = stats.Keys.FirstOrDefault() ?? "N5";
            }
            else
            {
                // Nhiều câu => chọn level có tỷ lệ đúng cao nhất, tie-break: nhiều câu hơn
                suggestedLevel = stats
                    .OrderByDescending(kv => kv.Value.total == 0 ? 0 : (double)kv.Value.correct / kv.Value.total)
                    .ThenByDescending(kv => kv.Value.total)
                    .Select(kv => kv.Key)
                    .FirstOrDefault() ?? "N5";
            }

            return new TestResultViewModel
            {
                CorrectAnswers = correct,
                TotalQuestions = total,
                SuggestedLevel = suggestedLevel
            };
        }
        private static int? NormalizeCorrectIndex(string? correctAnswer, List<string> choices)
        {
            if (string.IsNullOrWhiteSpace(correctAnswer)) return null;

            correctAnswer = correctAnswer.Trim();

            // --- Nếu là dạng "A. Gọi là" → cắt bỏ tiền tố "A." để lấy nội dung thực ---
            var answerText = (correctAnswer.Length > 2 && correctAnswer[1] == '.')
                ? correctAnswer.Substring(2).Trim()
                : correctAnswer;

            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i].Trim();

                // 1. So sánh toàn bộ (nếu bằng tuyệt đối thì đúng luôn)
                if (string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase))
                    return i;

                // 2. Cắt tiền tố "A.", "B.", ... từ lựa chọn và so sánh phần nội dung
                var choiceText = (choice.Length > 2 && choice[1] == '.')
                    ? choice.Substring(2).Trim()
                    : choice;

                if (string.Equals(choiceText, answerText, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return null;
        }



    }
}
