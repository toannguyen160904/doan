using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SharedModels;
using SharedModels.Models;
using SharedModels.Models.ViewModels;
using SharedModels.Models.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SharedModels.Models.ViewModels;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // LẤY CHI TIẾT BÀI HỌC
    [HttpGet("{id}")]
    public async Task<ActionResult<LessonDetailsViewModel>> GetLessonDetails(int id, int page = 1, int commentPage = 1)
    {
        int vocabPageSize = 5;
        int commentPageSize = 4;

        // Lấy bài học kèm từ vựng và ngữ pháp
        var lesson = await _context.Baihoc
            .Include(l => l.Level)
            .Include(l => l.tuvung)
            .Include(l => l.nguphap)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null) return NotFound();

        // Lấy ID từ vựng và ngữ pháp
        var vocabIds = lesson.tuvung.Select(v => v.Id).ToList();
        var grammarIds = lesson.nguphap.Select(g => g.Id).ToList();

        // Flashcard liên quan
        var flashcards = await _context.Flashcards
            .Include(f => f.Vocabulary)
            .Include(f => f.GrammarStructure)
            .Where(f =>
                (f.VocabularyId != null && vocabIds.Contains(f.VocabularyId.Value)) ||
                (f.GrammarStructureId != null && grammarIds.Contains(f.GrammarStructureId.Value)))
            .Where(f => f.Vocabulary != null || f.GrammarStructure != null)
            .ToListAsync();

        // Diễn đàn theo bài học
        var allComments = await _context.Diendan
            .Include(d => d.User)
            .Where(d => d.BaiHocId == id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var pagedComments = allComments
            .Skip((commentPage - 1) * commentPageSize)
            .Take(commentPageSize)
            .ToList();

        int totalCommentPages = (int)Math.Ceiling((double)allComments.Count / commentPageSize);

        // Phân trang từ vựng
        var pagedVocabularies = lesson.tuvung
            .Skip((page - 1) * vocabPageSize)
            .Take(vocabPageSize)
            .ToList();

        int totalVocabPages = (int)Math.Ceiling((double)lesson.tuvung.Count / vocabPageSize);

        // Lấy trạng thái học của user (nếu có đăng nhập)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var learnedVocabIds = new HashSet<int>();

        if (!string.IsNullOrEmpty(userId))
        {
            learnedVocabIds = await _context.UserVocabularyProgresses
                .Where(p => p.UserId == userId && p.IsLearned)
                .Select(p => p.VocabularyId)
                .ToHashSetAsync();
        }

        // Trả ViewModel
        var viewModel = new LessonDetailsViewModel
        {
            Lesson = lesson,
            Flashcards = flashcards,
            Diendan = allComments,

            PagedVocabularies = pagedVocabularies,
            CurrentVocabularyPage = page,
            TotalVocabularyPages = totalVocabPages,

            PagedComments = pagedComments,
            CurrentCommentPage = commentPage,
            TotalCommentPages = totalCommentPages,

            LearnedVocabularyIds = learnedVocabIds
        };

        return Ok(viewModel);
    }



    // ĐĂNG BÀI DIỄN ĐÀN
    [HttpPost("post")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
    {
        var userId = createPostDto.UserId;

        if (string.IsNullOrEmpty(userId))
            return BadRequest("Thiếu UserId.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Không tìm thấy người dùng.");

        var newPost = new Diendanmodel
        {
            BaiHocId = createPostDto.BaiHocId,
            UserId = userId,
            TieuDe = createPostDto.TieuDe,
            NoiDung = createPostDto.NoiDung,
            CreatedAt = DateTime.UtcNow
        };

        _context.Diendan.Add(newPost);
        await _context.SaveChangesAsync();

        var postViewModel = new PostViewModel
        {
            Id = newPost.Id,
            TieuDe = newPost.TieuDe,
            NoiDung = newPost.NoiDung,
            UserName = user.Name,
            CreatedAt = newPost.CreatedAt
        };

        return Ok(postViewModel);
    }

}
