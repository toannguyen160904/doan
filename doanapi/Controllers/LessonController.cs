using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using System.Linq;
using SharedModels.Models;
using SharedModels.Models.DTO;
using SharedModels.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace doanapi.Controllers;

[ApiController]
[Route("api/[controller]")] // => /api/lesson
public class LessonController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpClientFactory _httpClientFactory;

    public LessonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _userManager = userManager;
        _httpClientFactory = httpClientFactory;
    }

    // ======================= LESSONS CRUD =======================

    // GET /api/lesson?levelId=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? levelId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _context.Baihoc
            .AsNoTracking()
            .Include(b => b.Level)
            .OrderBy(b => b.LevelId)
            .ThenBy(b => b.Order)
            .AsQueryable();

        if (levelId.HasValue)
            query = query.Where(b => b.LevelId == levelId.Value);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.LevelId,
                Level = b.Level!.Name,
                b.Order,
                b.IsPreview,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        });
    }

    // GET /api/lesson/{id}/basic  (thông tin cơ bản)
    [HttpGet("{id:int}/basic")]
    public async Task<IActionResult> GetBasic(int id)
    {
        var lesson = await _context.Baihoc
            .AsNoTracking()
            .Include(x => x.Level)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lesson == null)
            return NotFound(new { message = "Lesson not found", id });

        return Ok(new
        {
            lesson.Id,
            lesson.Name,
            lesson.Title,
            lesson.LevelId,
            Level = lesson.Level?.Name,
            lesson.Order,
            lesson.IsPreview,
            lesson.CreatedAt
        });
    }

    // GET /api/lesson/{id}  (chi tiết + phân trang vocab/comment)
    // GET /api/lesson/{id}  (chi tiết + phân trang vocab/comment)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LessonDetailsViewModel>> GetDetails(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int commentPage = 1)
    {
        const int vocabPageSize = 5;
        const int commentPageSize = 4;

        page = Math.Max(1, page);
        commentPage = Math.Max(1, commentPage);

        // 1) Lấy thông tin cơ bản của bài (không Include danh sách lớn)
        var lessonBasic = await _context.Baihoc
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new
            {
                Lesson = l,
                LevelName = l.Level != null ? l.Level.Name : null
            })
            .FirstOrDefaultAsync();

        if (lessonBasic == null)
            return NotFound(new { message = "Lesson not found", id });

        // 2) Lấy vocab theo trang (trực tiếp trên DB)
        var vocabQuery = _context.tuvung
            .AsNoTracking()
            .Where(v => v.LessonId == id);

        var totalVocab = await vocabQuery.CountAsync();
        var pagedVocabularies = await vocabQuery
            .OrderBy(v => v.Id)
            .Skip((page - 1) * vocabPageSize)
            .Take(vocabPageSize)
            .ToListAsync();

        // 3) Lấy comment theo trang (trực tiếp trên DB)
        var commentsQuery = _context.Diendan
            .AsNoTracking()
            .Where(d => d.BaiHocId == id)
            .OrderByDescending(d => d.CreatedAt);

        var totalComments = await commentsQuery.CountAsync();
        var pagedComments = await commentsQuery
            .Skip((commentPage - 1) * commentPageSize)
            .Take(commentPageSize)
            .Include(d => d.User)               // chỉ để lấy tên hiển thị
            .AsSplitQuery()                     // tách query tránh join nặng
            .Select(d => new Diendanmodel      // hoặc DTO nhẹ nhàng hơn
            {
                Id = d.Id,
                BaiHocId = d.BaiHocId,
                UserId = d.UserId,
                TieuDe = d.TieuDe,
                NoiDung = d.NoiDung,
                CreatedAt = d.CreatedAt,
                User = d.User == null ? null : new ApplicationUser
                {
                    Id = d.User.Id,
                    Name = d.User.Name,
                    UserName = d.User.UserName
                }
            })
            .ToListAsync();

        // 4) Lấy flashcards (KHÔNG include sâu nếu không cần)
        var flashcards = await _context.Flashcards
            .AsNoTracking()
            .Where(f => f.BaihocId == id)
            .OrderBy(f => f.Id)
            .Select(f => new flashcards
            {
                Id = f.Id,
                BaihocId = f.BaihocId,
                VocabularyId = f.VocabularyId,
                GrammarStructureId = f.GrammarStructureId,
                CreatedAt = f.CreatedAt,
                // Nếu UI cần vài field của Vocabulary/Grammar thì project tối thiểu:
                Vocabulary = f.Vocabulary == null ? null : new Vocabulary
                {
                    Id = f.Vocabulary.Id,
                    Tuvung = f.Vocabulary.Tuvung,
                    Nghia = f.Vocabulary.Nghia,
                    HanTu = f.Vocabulary.HanTu
                },
                GrammarStructure = f.GrammarStructure == null ? null : new GrammarStructure
                {
                    Id = f.GrammarStructure.Id
                }
            })
            .ToListAsync();

        // 5) LearnedVocabularyIds: chỉ lọc theo vocab của bài và người dùng hiện tại
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        HashSet<int> learnedVocabIds = new();

        if (!string.IsNullOrEmpty(userId))
        {
            // chỉ lấy id vocab của bài để giảm tải
            var currentVocabIds = await _context.tuvung
                .AsNoTracking()
                .Where(v => v.LessonId == id)
                .Select(v => v.Id)
                .ToListAsync();

            learnedVocabIds = (await _context.UserVocabularyProgresses
                    .AsNoTracking()
                    .Where(p => p.UserId == userId &&
                                p.IsLearned &&
                                currentVocabIds.Contains(p.VocabularyId))
                    .Select(p => p.VocabularyId)
                    .ToListAsync())
                .ToHashSet();
        }

        // 6) Lắp vào ViewModel hiện có
        var vm = new LessonDetailsViewModel
        {
            Lesson = lessonBasic.Lesson, // Lesson entity (đã NoTracking)
            Flashcards = flashcards,      // đã tối ưu projection
                                          // Diendan: nếu bạn chỉ cần trang hiện tại thì gán paged; nếu UI cần tất cả thì cân nhắc đổi ViewModel
            Diendan = null,               // tránh nhét "tất cả" comment — có thể để null
            PagedVocabularies = pagedVocabularies,
            CurrentVocabularyPage = page,
            TotalVocabularyPages = (int)Math.Ceiling((double)totalVocab / vocabPageSize),
            PagedComments = pagedComments,
            CurrentCommentPage = commentPage,
            TotalCommentPages = (int)Math.Ceiling((double)totalComments / commentPageSize),
            LearnedVocabularyIds = learnedVocabIds
        };

        return Ok(vm);
    }


    // POST /api/lesson
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LessonCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required" });

        var levelExists = await _context.Levels.AnyAsync(l => l.Id == dto.LevelId);
        if (!levelExists)
            return NotFound(new { message = "Level not found", id = dto.LevelId });

        var entity = new Baihoc
        {
            Name = dto.Name.Trim(),
            Title = dto.Description,
            LevelId = dto.LevelId,
            Order = dto.Order ?? 0,
            IsPreview = dto.IsPreview ?? false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Baihoc.Add(entity);
        await _context.SaveChangesAsync();

        return Created($"/api/lesson/{entity.Id}", new
        {
            entity.Id,
            entity.Name,
            entity.Title,
            entity.LevelId,
            entity.Order,
            entity.IsPreview,
            entity.CreatedAt
        });
    }

    // PUT /api/lesson/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LessonUpdateDto dto)
    {
        var entity = await _context.Baihoc.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return NotFound(new { message = "Lesson not found", id });

        if (dto.LevelId.HasValue)
        {
            var levelOk = await _context.Levels.AnyAsync(l => l.Id == dto.LevelId.Value);
            if (!levelOk)
                return NotFound(new { message = "Level not found", id = dto.LevelId });
            entity.LevelId = dto.LevelId.Value;
        }

        if (dto.Name is not null) entity.Name = dto.Name.Trim();
        if (dto.Description is not null) entity.Title = dto.Description;
        if (dto.Order.HasValue) entity.Order = dto.Order.Value;
        if (dto.IsPreview.HasValue) entity.IsPreview = dto.IsPreview.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/lesson/{id}  (soft delete)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Baihoc.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return NotFound(new { message = "Lesson not found", id });

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ======================= FLASHCARDS BY LESSON =======================

    // GET /api/lesson/{lessonId}/flashcards?page=&pageSize=
    [HttpGet("{lessonId:int}/flashcards")]
    public async Task<IActionResult> GetFlashcardsByLesson(int lessonId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (lessonId <= 0) return BadRequest(new { message = "lessonId invalid" });
        (page, pageSize) = NormalizePaging(page, pageSize);

        var query = _context.Flashcards
            .Include(f => f.Vocabulary)
            .Include(f => f.GrammarStructure)
            .Where(f => f.BaihocId == lessonId)
            .AsNoTracking()
            .OrderBy(f => f.Id);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                f.Id,
                BaihocId = f.BaihocId,
                Vocabulary = f.Vocabulary == null ? null : new
                {
                    f.Vocabulary.Id,
                    f.Vocabulary.Tuvung,
                    f.Vocabulary.Nghia,
                    f.Vocabulary.HanTu
                },
                Grammar = f.GrammarStructure == null ? null : new
                {
                    f.GrammarStructure.Id
                },
                f.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        });
    }

    // POST /api/lesson/{lessonId}/flashcards
    [HttpPost("{lessonId:int}/flashcards")]
    public async Task<IActionResult> CreateFlashcardForLesson(int lessonId, [FromBody] CreateFlashcardDto dto)
    {
        if (lessonId <= 0) return BadRequest(new { message = "lessonId invalid" });
        if (dto is null) return BadRequest(new { message = "Body is required" });

        var validate = await ValidateFlashcardRefs(lessonId, dto.VocabularyId, dto.GrammarStructureId);
        if (!validate.Ok) return validate.Error!;

        var entity = new flashcards
        {
            BaihocId = lessonId,
            VocabularyId = dto.VocabularyId,
            GrammarStructureId = dto.GrammarStructureId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Flashcards.Add(entity);
        await _context.SaveChangesAsync();

        return Created($"/api/lesson/{lessonId}/flashcards", new
        {
            entity.Id,
            BaihocId = entity.BaihocId,
            entity.VocabularyId,
            entity.GrammarStructureId,
            entity.CreatedAt
        });
    }

    // PUT /api/lesson/flashcards/{id}
    [HttpPut("flashcards/{id:int}")]
    public async Task<IActionResult> UpdateFlashcard(int id, [FromBody] UpdateFlashcardDto dto)
    {
        var entity = await _context.Flashcards.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();

        if (dto.BaihocId.HasValue)
        {
            var exists = await _context.Baihoc.AnyAsync(x => x.Id == dto.BaihocId);
            if (!exists) return NotFound(new { message = "Lesson not found", id = dto.BaihocId });
            entity.BaihocId = dto.BaihocId;
        }

        if (dto.VocabularyId.HasValue && dto.GrammarStructureId.HasValue)
            return BadRequest(new { message = "Pick either VocabularyId OR GrammarStructureId" });

        if (dto.VocabularyId.HasValue)
        {
            var exists = await _context.tuvung.AnyAsync(x => x.Id == dto.VocabularyId);
            if (!exists) return NotFound(new { message = "Vocabulary not found", id = dto.VocabularyId });
            entity.VocabularyId = dto.VocabularyId;
            entity.GrammarStructureId = null;
        }

        if (dto.GrammarStructureId.HasValue)
        {
            var exists = await _context.nguphap.AnyAsync(x => x.Id == dto.GrammarStructureId);
            if (!exists) return NotFound(new { message = "Grammar not found", id = dto.GrammarStructureId });
            entity.GrammarStructureId = dto.GrammarStructureId;
            entity.VocabularyId = null;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/lesson/flashcards/{id}
    [HttpDelete("flashcards/{id:int}")]
    public async Task<IActionResult> DeleteFlashcard(int id)
    {
        var entity = await _context.Flashcards.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();

        _context.Flashcards.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ======================= FORUM (POSTS) =======================

    // POST /api/lesson/post
    [HttpPost("post")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
    {
        if (string.IsNullOrWhiteSpace(createPostDto.UserId))
            return BadRequest("Missing UserId.");

        var user = await _userManager.FindByIdAsync(createPostDto.UserId);
        if (user == null) return NotFound("User not found.");

        var newPost = new Diendanmodel
        {
            BaiHocId = createPostDto.BaiHocId,
            UserId = createPostDto.UserId,
            TieuDe = createPostDto.TieuDe,
            NoiDung = createPostDto.NoiDung,
            CreatedAt = DateTime.UtcNow
        };

        _context.Diendan.Add(newPost);
        await _context.SaveChangesAsync();

        return Ok(new PostViewModel
        {
            Id = newPost.Id,
            TieuDe = newPost.TieuDe,
            NoiDung = newPost.NoiDung,
            UserName = user.Name,
            CreatedAt = newPost.CreatedAt
        });
    }

    // ======================= HELPERS & DTOs =======================

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize, int max = 200)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > max) pageSize = max;
        return (page, pageSize);
    }

    private async Task<(bool Ok, IActionResult? Error)> ValidateFlashcardRefs(int lessonId, int? vocabularyId, int? grammarId)
    {
        if (vocabularyId is null && grammarId is null)
            return (false, BadRequest(new { message = "Provide VocabularyId OR GrammarStructureId" }));

        if (vocabularyId is not null && grammarId is not null)
            return (false, BadRequest(new { message = "Choose ONLY ONE: VocabularyId or GrammarStructureId" }));

        var lessonExists = await _context.Baihoc.AnyAsync(x => x.Id == lessonId);
        if (!lessonExists) return (false, NotFound(new { message = "Lesson not found", id = lessonId }));

        if (vocabularyId is int vid)
        {
            var exist = await _context.tuvung.AnyAsync(x => x.Id == vid);
            if (!exist) return (false, NotFound(new { message = "Vocabulary not found", id = vid }));
        }

        if (grammarId is int gid)
        {
            var exist = await _context.nguphap.AnyAsync(x => x.Id == gid);
            if (!exist) return (false, NotFound(new { message = "Grammar not found", id = gid }));
        }

        return (true, null);
    }

    // DTOs
   
}
