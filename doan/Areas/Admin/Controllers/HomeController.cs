using Microsoft.AspNetCore.Mvc;
using doan.Models;
using Microsoft.AspNetCore.Authorization;

namespace doan.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalUsers = _context.Users.Count();
            var totalLessons = _context.Baihoc.Count();
            var totalVocabs = _context.tuvung.Count();
            var totalGrammar = _context.nguphap.Count();
            var totalQuestions = _context.TestQuestions.Count();

            ViewBag.Users = totalUsers;
            ViewBag.Lessons = totalLessons;
            ViewBag.Vocabularies = totalVocabs;
            ViewBag.Grammars = totalGrammar;
            ViewBag.Questions = totalQuestions;

            return View();
        }
    }
}
