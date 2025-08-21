using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using SharedModels.Models;

namespace doanapi.Controllers;

[ApiController]
[Route("api/vocabularies")]
public class VocabulariesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public VocabulariesController(ApplicationDbContext ctx) => _context = ctx;

    // ========= LIST (đã có ở bạn) =========
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? lessonId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<Vocabulary> baseQuery = _context.tuvung.AsNoTracking();

        if (lessonId.HasValue)
        {
            var exists = await _context.Baihoc.AsNoTracking().AnyAsync(b => b.Id == lessonId.Value);
            if (!exists) return NotFound(new { message = "Lesson not found", id = lessonId });

            baseQuery = baseQuery.Where(v => v.LessonId == lessonId.Value);
        }

        var total = await baseQuery.CountAsync();
        var items = await baseQuery
            .OrderBy(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new { v.Id, v.Tuvung, v.Nghia, v.HanTu, v.AmHan, v.LessonId })
            .ToListAsync();

        return Ok(new { page, pageSize, total, totalPages = (int)Math.Ceiling(total / (double)pageSize), items });
    }



    // ========= GET ONE =========
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var v = await _context.tuvung.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound(new { message = "Vocabulary not found", id });

        return Ok(new { v.Id, v.Tuvung, v.Nghia, v.HanTu, v.AmHan });
    }

    // ========= CREATE =========
    // Body: { "Tuvung":"ねこ", "Nghia":"Con mèo", "HanTu":"猫", "AmHan":"Miêu", "LessonId": 1 }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVocabularyDto dto)
    {
        if (dto is null) return BadRequest(new { message = "Body is required" });
        if (string.IsNullOrWhiteSpace(dto.Tuvung) || string.IsNullOrWhiteSpace(dto.Nghia))
            return BadRequest(new { message = "Tuvung and Nghia are required" });
        if (!dto.LessonId.HasValue)
            return BadRequest(new { message = "LessonId is required" });

        var lessonExists = await _context.Baihoc.AnyAsync(b => b.Id == dto.LessonId.Value);
        if (!lessonExists) return NotFound(new { message = "Lesson not found", id = dto.LessonId });

        var entity = new Vocabulary
        {
            Tuvung = dto.Tuvung,
            Nghia = dto.Nghia,
            HanTu = dto.HanTu,
            AmHan = dto.AmHan,
            LessonId = dto.LessonId.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.tuvung.Add(entity);
        await _context.SaveChangesAsync();

        return Created($"/api/vocabularies/{entity.Id}", new
        {
            entity.Id,
            entity.Tuvung,
            entity.Nghia,
            entity.HanTu,
            entity.AmHan,
            entity.LessonId
        });
    }


    // ========= UPDATE =========
    // Body: { "Tuvung":"いぬ", "Nghia":"Con chó", "HanTu":"犬", "AmHan":"Khuyển", "LessonId": 1 }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVocabularyDto dto)
    {
        var v = await _context.tuvung.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound(new { message = "Vocabulary not found", id });

        if (dto.Tuvung != null) v.Tuvung = dto.Tuvung;
        if (dto.Nghia != null) v.Nghia = dto.Nghia;
        if (dto.HanTu != null) v.HanTu = dto.HanTu;
        if (dto.AmHan != null) v.AmHan = dto.AmHan;

        if (dto.LessonId.HasValue)
        {
            var lessonExists = await _context.Baihoc.AnyAsync(b => b.Id == dto.LessonId.Value);
            if (!lessonExists) return NotFound(new { message = "Lesson not found", id = dto.LessonId });
            v.LessonId = dto.LessonId.Value;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }


    // ========= DELETE =========
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _context.tuvung.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound(new { message = "Vocabulary not found", id });

        // dọn flashcards đang tham chiếu (nếu chưa cấu hình SetNull/Cascade)
        var fcs = await _context.Flashcards
                    .Where(f => f.VocabularyId == id)
                    .ToListAsync();
        if (fcs.Count > 0)
            _context.Flashcards.RemoveRange(fcs);

        _context.tuvung.Remove(v);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}

// ================= DTOs =================
public class CreateVocabularyDto
{
    public string Tuvung { get; set; } = default!;
    public string Nghia { get; set; } = default!;
    public string? HanTu { get; set; }
    public string? AmHan { get; set; }
    public int? LessonId { get; set; } // có thể để int bắt buộc: int LessonId {get;set;}
}

public class UpdateVocabularyDto
{
    public string? Tuvung { get; set; }
    public string? Nghia { get; set; }
    public string? HanTu { get; set; }
    public string? AmHan { get; set; }
    public int? LessonId { get; set; }
}

