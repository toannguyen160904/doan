using Microsoft.AspNetCore.Mvc;
using doan.Models;  // Use the doan.Models namespace to access data models
using System.Linq;

namespace doan.Controllers
{
    public class FlashcardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor to initialize the database context
        public FlashcardsController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Action to display flashcards for a specific lesson
        public IActionResult Flashcards(int baiHocId)
        {
            // Retrieve the lesson from the database
            var baiHoc = _context.Baihoc.FirstOrDefault(b => b.Id == baiHocId);

            // Check if the lesson exists
            if (baiHoc == null)
            {
                // If the lesson does not exist, return a 404 error
                return NotFound($"Không tìm thấy bài học với ID: {baiHocId}");
            }

            // Retrieve flashcards related to the lesson from the database
            var flashcards = _context.Flashcards
                                     .Where(f => f.BaihocId == baiHocId)
                                     .ToList();

            // Check if flashcards exist
            if (flashcards == null || !flashcards.Any())
            {
                // If no flashcards exist, set a message in ViewBag
                ViewBag.Message = "Chưa có flashcards cho bài học này.";
            }

            // Pass lesson and flashcards data to ViewData
            ViewData["BaiHocId"] = baiHocId;
            ViewData["BaiHocName"] = baiHoc.Name;
            ViewData["Flashcards"] = flashcards;

            // Return the view along with the list of flashcards
            return View("/Views/Home/Flashcards.cshtml");
        }
    }
}