using SharedModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SharedModels.Models;

namespace doan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TestQuestionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestQuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách câu hỏi
        public IActionResult Index()
        {
            var questions = _context.TestQuestions.ToList();
            return View(questions);
        }

        // GET: Form tạo câu hỏi
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lưu câu hỏi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TestQuestion model, string[] options)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Choices = options.ToList();
                    _context.TestQuestions.Add(model);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(model);
        }

        // GET: Sửa câu hỏi
        public IActionResult Edit(int id)
        {
            var question = _context.TestQuestions.Find(id);
            if (question == null)
                return NotFound();

            return View(question);
        }

        // POST: Lưu sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TestQuestion model, string[] options)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.TestQuestions.FirstOrDefault(q => q.Id == model.Id);
                if (existing == null)
                    return NotFound();

                existing.QuestionText = model.QuestionText;
                existing.CorrectAnswer = model.CorrectAnswer;
                existing.Topic = model.Topic;
                existing.Level = model.Level;
                existing.Explanation = model.Explanation;
                existing.IsActive = model.IsActive;
                existing.Score = model.Score;
                existing.Choices = options.ToList();

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Xóa
        public IActionResult Delete(int id)
        {
            var question = _context.TestQuestions.Find(id);
            if (question == null)
                return NotFound();

            _context.TestQuestions.Remove(question);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        // GET: Xem chi tiết câu hỏi
        public IActionResult Details(int id)
        {
            var question = _context.TestQuestions.FirstOrDefault(q => q.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

    }
}
