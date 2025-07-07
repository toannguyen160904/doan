using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SharedModels;
using System;
using System.Linq;
using SharedModels.Models;

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
    public async Task<IActionResult> DangBai(int BaiHocId, string TieuDe, string NoiDung)
    {
        // Kiểm tra dữ liệu đầu vào
        if (string.IsNullOrWhiteSpace(NoiDung))
        {
            return BadRequest(new { success = false, message = "Nội dung bình luận không được để trống." });
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để thực hiện hành động này." });
        }

        var user = await _userManager.FindByIdAsync(userId);

        var newPost = new Diendanmodel
        {
            BaiHocId = BaiHocId,
            UserId = userId,
            TieuDe = TieuDe, // Dù ẩn nhưng vẫn cần
            NoiDung = NoiDung,
            CreatedAt = DateTime.Now
        };

        _context.Diendan.Add(newPost);
        await _context.SaveChangesAsync();

        // Trả về dữ liệu của bài viết mới dưới dạng JSON
        // Đây là bước quan trọng nhất
        return Ok(new
        {
            success = true,
            post = new
            {
                id = newPost.Id,
                tieuDe = newPost.TieuDe,
                noiDung = newPost.NoiDung,
                userName = user.Name, // Lấy tên người dùng
                createdAt = newPost.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            }
        });
    }
}
