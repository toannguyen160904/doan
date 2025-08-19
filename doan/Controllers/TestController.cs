using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;                 // ApplicationDbContext
using SharedModels.Models;          // TestQuestion
using DTO = SharedModels.Models.DTO;
using System.Text.Json;
using SharedModels.Models.ViewModels;

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

        public IActionResult Start()
        {
            HttpContext.Session.Remove(TestSessionKey);
            return View();
        }

        public async Task<IActionResult> DoTest()
        {
            var ids = await GetOrCreateTestQuestionIdsAsync();
            if (ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Không có câu hỏi nào trong hệ thống.";
                return RedirectToAction(nameof(Start));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(List<DTO.TestAnswerInput> answers)
        {
            HttpContext.Session.Remove(TestSessionKey);
            var correct = await CalculateScoreAsync(answers);
            var level = DetermineLevel(correct);

            var vm = new TestResultViewModel
            {
                CorrectAnswers = correct,
                TotalQuestions = answers?.Count ?? 0,
                SuggestedLevel = level
            };

            return View("Result", vm);
        }

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
                if (!map.TryGetValue(ans.QuestionId, out var q) || q == null) continue;

                var choices = q.Choices ?? new(); // ✅ Dùng model đã deserialize sẵn

                int? selectedIndex = GetSelectedIndex(ans, choices);
                if (selectedIndex is null) continue;

                int? correctIndex = NormalizeCorrectIndex(q.CorrectAnswer, choices);
                if (correctIndex is null) continue;

                if (selectedIndex.Value == correctIndex.Value) correct++;
            }


            return correct;
        }

        private static int? GetSelectedIndex(object ans, List<string> choices)
        {
            var saProp = ans.GetType().GetProperty("SelectedAnswer");
            if (saProp?.GetValue(ans) is string sa && !string.IsNullOrWhiteSpace(sa))
            {
                var idx = ParseIndexFromString(sa, choices);
                if (idx != null) return idx;
            }

            var siProp = ans.GetType().GetProperty("SelectedIndex");
            if (siProp != null)
            {
                var val = siProp.GetValue(ans);
                if (val is int si && si >= 0 && si < choices.Count) return si;
                if (val is string sis && !string.IsNullOrWhiteSpace(sis))
                {
                    var idx = ParseIndexFromString(sis, choices);
                    if (idx != null) return idx;
                }
            }
            return null;
        }

        private static int? ParseIndexFromString(string raw, List<string> choices)
        {
            raw = raw.Trim();

            if (int.TryParse(raw, out var n0) && n0 >= 0 && n0 < choices.Count) return n0;
            if (int.TryParse(raw, out var n1) && n1 >= 1 && n1 <= choices.Count) return n1 - 1;

            char c = char.ToUpperInvariant(raw[0]);
            if (c >= 'A' && c <= 'Z')
            {
                int idx = c - 'A';
                if (idx >= 0 && idx < choices.Count) return idx;
            }

            var idxText = choices.FindIndex(x =>
                string.Equals(x?.Trim(), raw, StringComparison.OrdinalIgnoreCase));
            return idxText >= 0 ? idxText : (int?)null;
        }

        private static int? NormalizeCorrectIndex(string? correctAnswer, List<string> choices)
        {
            if (string.IsNullOrWhiteSpace(correctAnswer)) return null;
            correctAnswer = correctAnswer.Trim();

            if (int.TryParse(correctAnswer, out var n))
            {
                if (n >= 1 && n <= choices.Count) return n - 1;
                if (n >= 0 && n < choices.Count) return n;
                return null;
            }

            char c = char.ToUpperInvariant(correctAnswer[0]);
            if (c >= 'A' && c <= 'Z')
            {
                int idx = c - 'A';
                if (idx >= 0 && idx < choices.Count) return idx;
            }

            var idxText = choices.FindIndex(x =>
                string.Equals(x?.Trim(), correctAnswer, StringComparison.OrdinalIgnoreCase));
            return idxText >= 0 ? idxText : (int?)null;
        }

        private static string DetermineLevel(int correctCount) => correctCount switch
        {
            >= 15 => "N3",
            >= 8 => "N4",
            _ => "N5"
        };
    }
}
