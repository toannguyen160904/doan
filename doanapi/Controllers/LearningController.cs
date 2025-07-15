// File: doanapi/Controllers/LearningController.cs (ĐÃ CHỈNH SỬA)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedModels; // Đảm bảo using này đúng
using SharedModels.Models; // Đảm bảo using này đúng
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SharedModels.Models.ViewModels;
using SharedModels.Models.DTO;


// 1. Thêm các attribute [ApiController] và [Route]
[ApiController]
[Route("api/learning")]
[Authorize] // Giữ lại Authorize để bảo vệ các endpoint này
// 2. Kế thừa từ ControllerBase
public class LearningController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LearningController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 3. Đổi tên Action và thêm [HttpGet]
    // Endpoint này sẽ trả về một object chứa toàn bộ dữ liệu cho trang học hàng ngày
    [HttpGet("daily")] // URL: /api/learning/daily
    public async Task<ActionResult<DailyLearningViewModel>> GetDailyPlan()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(); // Nếu không tìm thấy user, trả về lỗi 401
        }

        var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);

        // Logic khởi tạo plan và reset plan hàng ngày giữ nguyên
        if (plan == null)
        {
            plan = new UserLearningPlan
            {
                UserId = user.Id,
                DailyTarget = 10,
                CompletedToday = 0,
                LastUpdated = DateTime.Now
            };
            _context.UserLearningPlans.Add(plan);
            await _context.SaveChangesAsync();
        }

        if (plan.LastUpdated.Date != DateTime.Today)
        {
            plan.CompletedToday = 0;
            plan.LastUpdated = DateTime.Now;
            _context.UserLearningPlans.Update(plan);
            await _context.SaveChangesAsync();
        }

        var learnedIds = await _context.UserVocabularyProgresses
            .Where(p => p.UserId == user.Id && p.IsLearned)
            .Select(p => p.VocabularyId)
            .ToListAsync();

        var newVocabList = await _context.tuvung
            .Where(v => !learnedIds.Contains(v.Id))
            .OrderBy(v => Guid.NewGuid()) // Lưu ý: OrderBy(Guid.NewGuid()) có thể không hiệu quả trên DB lớn
            .Take(10)
            .ToListAsync();

        var reviewList = await _context.UserVocabularyProgresses
            .Where(p => p.UserId == user.Id && p.IsLearned && p.NextReviewDate <= DateTime.Today)
            .Include(p => p.Vocabulary)
               
            .ToListAsync();

        // 4. Tạo một ViewModel để đóng gói và trả về tất cả dữ liệu cần thiết
        var viewModel = new DailyLearningViewModel
        {
            CompletedToday = plan.CompletedToday,
            DailyTarget = plan.DailyTarget,
            NewWords = newVocabList,
            ReviewWords = reviewList
        };

        // 5. Trả về dữ liệu dưới dạng JSON với mã 200 OK
        return Ok(viewModel);
    }

    // Endpoint này để đánh dấu một từ đã học
    [HttpPost("mark-learned")] // URL: /api/learning/mark-learned
    public async Task<IActionResult> MarkLearned([FromBody] VocabularyMarkRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var vocabId = request.VocabId;
        var progress = await _context.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == vocabId);

        var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (plan == null) return BadRequest("User learning plan not found.");

        if (progress == null)
        {
            progress = new UserVocabularyProgress
            {
                UserId = user.Id,
                VocabularyId = vocabId,
                IsLearned = true,
                LearnedDate = DateTime.Now,
                ReviewLevel = 1,
                NextReviewDate = DateTime.Now.AddDays(1)
            };
            _context.UserVocabularyProgresses.Add(progress);
        }
        else
        {
            progress.IsLearned = true;
            progress.LearnedDate = DateTime.Now;
            // Logic tính ngày ôn tập có thể được cải thiện thành một service riêng
            switch (progress.ReviewLevel)
            {
                case 0: progress.NextReviewDate = DateTime.Now.AddDays(1); break;
                case 1: progress.NextReviewDate = DateTime.Now.AddDays(3); break;
                case 2: progress.NextReviewDate = DateTime.Now.AddDays(7); break;
                default: progress.NextReviewDate = DateTime.Now.AddDays(14); break;
            }
            progress.ReviewLevel++;
            _context.UserVocabularyProgresses.Update(progress);
        }

        if (plan.CompletedToday < plan.DailyTarget)
        {
            plan.CompletedToday++;
            _context.UserLearningPlans.Update(plan);
        }

        await _context.SaveChangesAsync();
        // 6. Trả về 204 NoContent, báo hiệu thành công nhưng không cần nội dung trả về
        return NoContent();
    }

    // Endpoint này để đánh dấu một từ chưa học (hoặc quên)
    [HttpPost("mark-unlearned")] // URL: /api/learning/mark-unlearned
    public async Task<IActionResult> MarkUnlearned([FromBody] VocabularyMarkRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var vocabId = request.VocabId;
        var progress = await _context.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == vocabId);

        if (progress != null && progress.IsLearned)
        {
            progress.IsLearned = false;
            progress.LearnedDate = null;
            progress.ReviewLevel = 0; // Reset mức độ ôn tập
            progress.NextReviewDate = null;
            _context.UserVocabularyProgresses.Update(progress);

            var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (plan != null && plan.CompletedToday > 0)
            {
                plan.CompletedToday--;
                _context.UserLearningPlans.Update(plan);
            }

            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    // Endpoint để lấy danh sách tất cả các từ đã học
    [HttpGet("learned-words")] // URL: /api/learning/learned-words
    public async Task<ActionResult<IEnumerable<Vocabulary>>> GetLearnedWords()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var learnedWords = await _context.UserVocabularyProgresses
            .Where(p => p.UserId == user.Id && p.IsLearned)
            .Include(p => p.Vocabulary)
            .Select(p => p.Vocabulary)
            .ToListAsync();

        return Ok(learnedWords);
    }
}

