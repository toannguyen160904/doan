using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.AspNetCore.Identity;

public class LessonController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Details(int id)
    {
        if (_context == null)
        {
            return Problem("Database context is not available.");
        }

        var lesson = _context.Baihoc
            .Include(l => l.tuvung)
            .Include(l => l.nguphap)
            .Include(l => l.Diendan)
            .ThenInclude(d => d.User)
            .FirstOrDefault(l => l.Id == id);

        if (lesson == null)
        {
            return NotFound();
        }

        // Lấy danh sách flashcard từ các từ vựng và ngữ pháp của bài học
        var vocabIds = lesson.tuvung.Select(v => v.Id).ToList();
        var grammarIds = lesson.nguphap.Select(g => g.Id).ToList();

        var flashcards = _context.Flashcards
            .Include(f => f.Vocabulary)
            .Include(f => f.GrammarStructure)
            .Where(f =>
                (f.VocabularyId != null && vocabIds.Contains(f.VocabularyId.Value)) ||
                (f.GrammarStructureId != null && grammarIds.Contains(f.GrammarStructureId.Value)))
            .ToList();

        ViewBag.Flashcards = flashcards;

        return View("~/Views/Level/Details.cshtml", lesson);
    }
    [HttpPost]
    public IActionResult DangBai(int BaiHocId, string TieuDe, string NoiDung)
    {
        if (ModelState.IsValid)
        {
            var baiHoc = _context.Baihoc
                .Include(b => b.Diendan)
                .FirstOrDefault(b => b.Id == BaiHocId);
            if (baiHoc != null)
            {
                var userId = _userManager.GetUserId(User);
                var newPost = new Diendanmodel
                {
                    BaiHocId = BaiHocId,
                    UserId = userId,
                    TieuDe = TieuDe,
                    NoiDung = NoiDung,
                    CreatedAt = DateTime.Now
                };

                baiHoc.Diendan.Add(newPost);
                _context.SaveChanges();

                return RedirectToAction("Details", new { id = BaiHocId });
            }
        }

        return RedirectToAction("Index");
    }
}
