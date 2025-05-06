using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using doan.Models;
using System;
using System.Linq;

public class LessonController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Hiển thị chi tiết bài học
    public IActionResult Details(int id)
    {
        // Lấy bài học theo ID, bao gồm từ vựng, ngữ pháp và diễn đàn liên quan
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

        // Lấy danh sách ID từ vựng và ngữ pháp của bài học
        var vocabIds = lesson.tuvung.Select(v => v.Id).ToList();
        var grammarIds = lesson.nguphap.Select(g => g.Id).ToList();

        // Lấy các flashcard liên quan đến từ vựng hoặc ngữ pháp
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

    // Đăng bài viết lên diễn đàn của bài học
    [HttpPost]
    public IActionResult DangBai(int BaiHocId, string TieuDe, string NoiDung)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Details", new { id = BaiHocId });
        }

        var baiHoc = _context.Baihoc
            .Include(b => b.Diendan)
            .FirstOrDefault(b => b.Id == BaiHocId);

        if (baiHoc == null)
        {
            return NotFound();
        }

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
