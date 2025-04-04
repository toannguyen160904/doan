using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using doan.Models;

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

        // ✅ Trang danh sách từ vựng
        public IActionResult Index()
        {
            var vocabularies = _context.tuvung.ToList();
            return View(vocabularies);
        }

        // ✅ Tạo từ vựng (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Tạo từ vựng (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Vocabulary model)
        {
            if (ModelState.IsValid)
            {
                _context.tuvung.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
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
