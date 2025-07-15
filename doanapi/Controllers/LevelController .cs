// File: doanapi/Controllers/LevelController.cs (ĐÃ CHỈNH SỬA)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using SharedModels.Models;
using SharedModels.Models.ViewModels; // Thêm using cho ViewModel
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

// 1. Thêm các attribute chuẩn
[ApiController]
[Route("api/[controller]")]
public class LevelController : ControllerBase // 2. Kế thừa từ ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LevelController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 3. Chuyển đổi Action 'ChonLevel' thành một endpoint RESTful
    // Endpoint này sẽ lấy danh sách bài học theo tên cấp độ (N5, N4, ...)
    [HttpGet("lesson-details/{id}")]
    public async Task<ActionResult<LessonDetailsViewModel>> GetLessonDetails(int id)
    {
        var lesson = await _context.Baihoc
            .Include(l => l.Level)
            .Include(l => l.tuvung)
            .Include(l => l.nguphap)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null)
            return NotFound();

        var vocabIds = lesson.tuvung.Select(v => v.Id).ToList();
        var grammarIds = lesson.nguphap.Select(g => g.Id).ToList();

        var flashcards = await _context.Flashcards
            .Include(f => f.Vocabulary)
            .Include(f => f.GrammarStructure)
            .Where(f =>
                (f.VocabularyId != null && vocabIds.Contains(f.VocabularyId.Value)) ||
                (f.GrammarStructureId != null && grammarIds.Contains(f.GrammarStructureId.Value)))
            .Where(f => f.Vocabulary != null || f.GrammarStructure != null)
            .ToListAsync();

        // TÁCH QUERIES RIÊNG CHO DIỄN ĐÀN
        var diendanList = await _context.Diendan
            .Where(d => d.BaiHocId == lesson.Id)
            .Include(d => d.User) // load tên người dùng
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var viewModel = new LessonDetailsViewModel
        {
            Lesson = lesson,
            Flashcards = flashcards,
            Diendan = diendanList
        };

        return Ok(viewModel);
    }


    // Thêm một endpoint để lấy tất cả các cấp độ
    [HttpGet] // URL: /api/level
    public async Task<ActionResult<IEnumerable<Level>>> GetAllLevels()
    {
        var allLevels = await _context.Levels.ToListAsync();
        return Ok(allLevels);
    }


    // 6. Chuyển đổi Action 'Details'
    // Endpoint này lấy chi tiết một cấp độ và danh sách bài học của nó
    [HttpGet("details/{id}")]// URL: /api/level/1
    public async Task<IActionResult> GetLevelDetails(int id)
    {
        // Khi dùng API, cần cẩn thận với Include lồng nhau để tránh lỗi lặp vô hạn (circular reference)
        // Bỏ ThenInclude(b => b.Level) vì client đã biết nó đang ở trong Level nào rồi.
        var level = await _context.Levels
            .Include(l => l.Lessons)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (level == null)
        {
            return NotFound();
        }

        return Ok(level);
    }

    // 7. Giữ lại hàm helper private này
    private int GetLevelId(string level) => level.ToUpper() switch
    {
        "N5" => 1,
        "N4" => 2,
        "N3" => 3,
        // Có thể thêm các cấp độ khác ở đây
        _ => 1 // Mặc định là N5
    };
}