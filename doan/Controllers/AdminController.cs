using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using SharedModels.Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;
using SharedModels.Models.ViewModels;
using SharedModels.Models;

namespace doan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");
            return View();
        }

        // POST: /Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Vocabulary model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _context.tuvung.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Levels = new SelectList(_context.Levels, "Id", "Name");

            // In lỗi nếu có
            foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("❌ Lỗi ModelState: " + err.ErrorMessage);
            }

            return View(model);
        }
        // ✅ Chỉnh sửa từ vựng (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vocab = _context.tuvung.FirstOrDefault(v => v.Id == id);
            if (vocab == null)
            {
                return NotFound();
            }
            return View(vocab);
        }

        // ✅ Chỉnh sửa từ vựng (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Vocabulary model)
        {
            if (ModelState.IsValid)
            {
                _context.tuvung.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // ✅ Xác nhận xóa (GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var vocab = _context.tuvung.FirstOrDefault(v => v.Id == id);
            if (vocab == null)
            {
                return NotFound();
            }
            return View(vocab);
        }

        // ✅ Xóa (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var vocab = _context.tuvung.FirstOrDefault(v => v.Id == id);
            if (vocab == null)
            {
                return NotFound();
            }

            _context.tuvung.Remove(vocab);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
