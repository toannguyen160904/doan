using doan.Models;
using doan.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace doan.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Start()
        {
            return View(); // Mặc định trả về Views/Test/Start.cshtml
        }

        public async Task<IActionResult> DoTest()
        {
            var questions = await _context.TestQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => Guid.NewGuid())
                .Take(20)
                .ToListAsync();

            return View(questions); // Views/Test/DoTest.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> Submit(List<TestAnswerInput> answers)
        {
            int correct = 0;
            foreach (var ans in answers)
            {
                var q = await _context.TestQuestions.FindAsync(ans.QuestionId);
                if (q != null && q.CorrectAnswer == ans.SelectedAnswer)
                    correct++;
            }

            string level = correct switch
            {
                >= 15 => "N3",
                >= 8 => "N4",
                _ => "N5"
            };

            var result = new TestResult
            {
                CorrectAnswers = correct,
                TotalScore = correct,
                SuggestedLevel = level
            };

            return View("Result", result); // Views/Test/Result.cshtml
        }
    }
}
