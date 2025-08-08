
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;                 // ApplicationDbContext
using SharedModels.Models;          // TestQuestion, TestResult
using DTO = SharedModels.Models.DTO; // <-- alias cho DTO
using System.Text.Json;

namespace doan.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string TestSessionKey = "TestQuestionIds";
        private const int NumberOfTestQuestions = 20;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /Test/Start
        public IActionResult Start()
        {
            HttpContext.Session.Remove(TestSessionKey);
            return View();
        }

        // GET /Test/DoTest
        public async Task<IActionResult> DoTest()
        {
            var ids = await GetOrCreateTestQuestionIdsAsync();

            if (ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Không có câu hỏi nào trong hệ thống.";
                return RedirectToAction("Start");
            }

            var questions = await _context.TestQuestions
                .Where(q => ids.Contains(q.Id))
                .ToListAsync();

            var ordered = ids
                .Select(id => questions.FirstOrDefault(q => q.Id == id))
                .Where(q => q != null)
                .ToList();

            return View(ordered);
        }

        // POST /Test/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(List<DTO.TestAnswerInput> answers) // <-- dùng alias DTO
        {
            HttpContext.Session.Remove(TestSessionKey);

            var correct = await CalculateScoreAsync(answers);
            var level = DetermineLevel(correct);

            var result = new TestResult
            {
                CorrectAnswers = correct,
                TotalScore = correct,
                SuggestedLevel = level
            };

            return View("Result", result);
        }

        // ===== Helpers =====
        private async Task<List<int>> GetOrCreateTestQuestionIdsAsync()
        {
            var json = HttpContext.Session.GetString(TestSessionKey);
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();

            var newIds = await _context.TestQuestions
                .Where(q => q.IsActive)
                .OrderBy(_ => EF.Functions.Random())
                .Take(NumberOfTestQuestions)
                .Select(q => q.Id)
                .ToListAsync();

            HttpContext.Session.SetString(TestSessionKey, JsonSerializer.Serialize(newIds));
            return newIds;
        }

        private async Task<int> CalculateScoreAsync(List<DTO.TestAnswerInput> answers)
        {
            if (answers == null || answers.Count == 0) return 0;

            int correct = 0;
            var ids = answers.Select(a => a.QuestionId).ToList();

            var map = await _context.TestQuestions
                .Where(q => ids.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            foreach (var ans in answers)
            {
                if (!map.TryGetValue(ans.QuestionId, out var q)) continue;

                // 1) Selected từ client (index hoặc text)
                string? selectedText = null;
                int? selectedIndex = null;

                if (int.TryParse(ans.SelectedAnswer, out var selIdx) &&
                    selIdx >= 0 && selIdx < q.Choices.Count)
                {
                    selectedIndex = selIdx;
                    selectedText = q.Choices[selIdx];
                }
                else
                {
                    selectedText = ans.SelectedAnswer?.Trim();
                }

                // 2) Đáp án đúng trong DB (index hoặc text)
                bool isCorrect = false;

                // a) DB lưu index (ví dụ "2")
                if (int.TryParse(q.CorrectAnswer, out var correctIdx))
                {
                    if (selectedIndex.HasValue && selectedIndex.Value == correctIdx)
                        isCorrect = true;
                }
                else
                {
                    // b) DB lưu text
                    var correctText = q.CorrectAnswer?.Trim();
                    if (!string.IsNullOrEmpty(correctText) && !string.IsNullOrEmpty(selectedText) &&
                        string.Equals(correctText, selectedText, StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrect = true;
                    }
                    else if (selectedIndex.HasValue)
                    {
                        // fallback: so sánh theo index tính từ text đúng
                        var idxFromText = q.Choices.FindIndex(c =>
                            string.Equals(c?.Trim(), correctText, StringComparison.OrdinalIgnoreCase));
                        if (idxFromText >= 0 && idxFromText == selectedIndex.Value)
                            isCorrect = true;
                    }
                }

                if (isCorrect) correct++;
            }

            return correct;
        }

        private string DetermineLevel(int correctCount) => correctCount switch
        {
            >= 15 => "N3",
            >= 8 => "N4",
            _ => "N5"
        };
    }
}
