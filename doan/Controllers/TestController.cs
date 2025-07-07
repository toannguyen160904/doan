using SharedModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace doan.Controllers
{
    /// <summary>
    /// Xử lý logic cho việc làm bài kiểm tra đầu vào của người dùng.
    /// </summary>
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string TestSessionKey = "TestQuestionIds";
        private const int NumberOfTestQuestions = 20;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hiển thị trang bắt đầu, xóa session bài test cũ nếu có.
        /// </summary>
        [Authorize]
        public IActionResult Start()
        {
            HttpContext.Session.Remove(TestSessionKey);
            return View();
        }

        /// <summary>
        /// Hiển thị trang làm bài test với một bộ câu hỏi cố định cho phiên làm bài.
        /// </summary>
        public async Task<IActionResult> DoTest()
        {
            // Lấy danh sách ID câu hỏi từ session hoặc tạo mới nếu chưa có.
            var questionIds = await GetOrCreateTestQuestionIdsAsync();

            if (!questionIds.Any())
            {
                // Xử lý trường hợp không có câu hỏi nào để hiển thị.
                TempData["ErrorMessage"] = "Không có câu hỏi nào trong hệ thống.";
                return RedirectToAction("Start");
            }

            // Dùng danh sách ID để lấy đầy đủ thông tin các câu hỏi.
            var questions = await _context.TestQuestions
                                        .Where(q => questionIds.Contains(q.Id))
                                        .ToListAsync();

            // Sắp xếp lại danh sách câu hỏi theo đúng thứ tự đã lưu trong session.
            var orderedQuestions = questionIds
                .Select(id => questions.FirstOrDefault(q => q.Id == id))
                .Where(q => q != null) // Lọc bỏ câu hỏi có thể đã bị xóa khỏi DB.
                .ToList();

            return View(orderedQuestions);
        }

        /// <summary>
        /// Nhận và xử lý bài làm của người dùng, sau đó hiển thị kết quả.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Submit(List<TestAnswerInput> answers)
        {
            // Xóa session khi nộp bài.
            HttpContext.Session.Remove(TestSessionKey);

            // Tách logic chấm điểm ra một phương thức riêng.
            int correctAnswersCount = await CalculateScoreAsync(answers);

            // Tách logic xác định trình độ.
            string suggestedLevel = DetermineLevel(correctAnswersCount);

            var result = new TestResult
            {
                CorrectAnswers = correctAnswersCount,
                TotalScore = correctAnswersCount, // Giả sử mỗi câu 1 điểm
                SuggestedLevel = suggestedLevel
            };

            return View("Result", result);
        }

        #region Private Helper Methods

        /// <summary>
        /// Lấy danh sách ID câu hỏi từ Session. Nếu không có, tạo mới và lưu vào Session.
        /// </summary>
        private async Task<List<int>> GetOrCreateTestQuestionIdsAsync()
        {
            var questionIdsJson = HttpContext.Session.GetString(TestSessionKey);

            if (!string.IsNullOrEmpty(questionIdsJson))
            {
                // Đọc từ session nếu đã có.
                return JsonSerializer.Deserialize<List<int>>(questionIdsJson);
            }

            // Tạo mới nếu chưa có trong session.
            var newQuestionIds = await _context.TestQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => Guid.NewGuid())
                .Take(NumberOfTestQuestions)
                .Select(q => q.Id)
                .ToListAsync();

            // Lưu vào session để sử dụng cho các lần reload sau.
            HttpContext.Session.SetString(TestSessionKey, JsonSerializer.Serialize(newQuestionIds));

            return newQuestionIds;
        }

        /// <summary>
        /// Tính toán số câu trả lời đúng từ danh sách câu trả lời của người dùng.
        /// </summary>
        private async Task<int> CalculateScoreAsync(List<TestAnswerInput> answers)
        {
            if (answers == null || !answers.Any()) return 0;

            int correctCount = 0;
            var questionIds = answers.Select(a => a.QuestionId).ToList();

            // Lấy tất cả câu hỏi liên quan trong một lần truy vấn DB.
            var questionsInTest = await _context.TestQuestions
                                                .Where(q => questionIds.Contains(q.Id))
                                                .ToDictionaryAsync(q => q.Id);

            foreach (var ans in answers)
            {
                if (questionsInTest.TryGetValue(ans.QuestionId, out var question)
                    && int.TryParse(ans.SelectedAnswer, out int choiceIndex))
                {
                    if (choiceIndex >= 0 && choiceIndex < question.Choices.Count)
                    {
                        string selectedChoiceText = question.Choices[choiceIndex];
                        if (question.CorrectAnswer == selectedChoiceText)
                        {
                            correctCount++;
                        }
                    }
                }
            }

            return correctCount;
        }

        /// <summary>
        /// Xác định trình độ gợi ý dựa trên số câu đúng.
        /// </summary>
        private string DetermineLevel(int correctCount)
        {
            return correctCount switch
            {
                >= 15 => "N3",
                >= 8 => "N4",
                _ => "N5"
            };
        }

        #endregion
    }
}