// File: doanapi/Controllers/LearningController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using SharedModels.Models;
using SharedModels.Models.DTO;
using SharedModels.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace doanapi.Controllers
{
    /// <summary>
    /// API quản lý lộ trình học từ vựng, đánh dấu trạng thái học, và ôn tập.
    /// </summary>
    [ApiController]
    [Route("api/learning")]
    [Authorize] // Bảo vệ toàn bộ endpoint, yêu cầu đăng nhập
    public class LearningController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LearningController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy lộ trình học hôm nay (từ mới và từ cần ôn tập)
        /// GET /api/learning/daily
        /// </summary>
        [HttpGet("daily")]
        public async Task<ActionResult<DailyLearningViewModel>> GetDailyPlan()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var nowUtc = DateTime.UtcNow;
            var startOfTodayUtc = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfTodayUtc = startOfTodayUtc.AddDays(1);

            // Tìm hoặc tạo mới lộ trình học của user
            var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (plan == null)
            {
                plan = new UserLearningPlan
                {
                    UserId = user.Id,
                    DailyTarget = 10,
                    CompletedToday = 0,
                    LastUpdated = nowUtc
                };
                _context.UserLearningPlans.Add(plan);
                await _context.SaveChangesAsync();
            }

            // Reset tiến độ nếu sang ngày mới
            if (plan.LastUpdated < startOfTodayUtc || plan.LastUpdated >= endOfTodayUtc)
            {
                plan.CompletedToday = 0;
                plan.LastUpdated = nowUtc;
                _context.UserLearningPlans.Update(plan);
                await _context.SaveChangesAsync();
            }

            // Lấy danh sách ID từ đã học
            var learnedIds = await _context.UserVocabularyProgresses
                .Where(p => p.UserId == user.Id && p.IsLearned)
                .Select(p => p.VocabularyId)
                .ToListAsync();

            // Lấy 10 từ mới random chưa học
            var newVocabList = await _context.tuvung
                .Where(v => !learnedIds.Contains(v.Id))
                .OrderBy(_ => EF.Functions.Random()) // Random Postgres
                .Take(10)
                .ToListAsync();

            // Lấy danh sách từ cần ôn lại (đến hạn ôn)
            var reviewList = await _context.UserVocabularyProgresses
                .Where(p => p.UserId == user.Id
                            && p.IsLearned
                            && p.NextReviewDate.HasValue
                            && p.NextReviewDate <= nowUtc)
                .Include(p => p.Vocabulary)
                .ToListAsync();

            // Trả về ViewModel
            var viewModel = new DailyLearningViewModel
            {
                CompletedToday = plan.CompletedToday,
                DailyTarget = plan.DailyTarget,
                NewWords = newVocabList,
                ReviewWords = reviewList
            };

            return Ok(viewModel);
        }

        /// <summary>
        /// Đánh dấu từ là đã học
        /// POST /api/learning/mark-learned
        /// Body: { "vocabId": 123 }
        /// </summary>
        [HttpPost("mark-learned")]
        public async Task<IActionResult> MarkLearned([FromBody] VocabularyMarkRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState); // 400 cho abc, 0, null...

            // ✅ Kiểm tra từ vựng có tồn tại để tránh FK 500
            bool vocabExists = await _context.tuvung.AsNoTracking()
                .AnyAsync(v => v.Id == request.VocabId);
            if (!vocabExists)
                return NotFound(new { message = "Vocabulary not found", id = request.VocabId });

            var nowUtc = DateTime.UtcNow;

            var progress = await _context.UserVocabularyProgresses
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == request.VocabId);

            var plan = await _context.UserLearningPlans
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (plan == null)
                return BadRequest(new { message = "User learning plan not found" });

            bool wasLearned = progress?.IsLearned == true;

            if (progress == null)
            {
                progress = new UserVocabularyProgress
                {
                    UserId = user.Id,
                    VocabularyId = request.VocabId,
                    IsLearned = true,
                    LearnedDate = nowUtc,
                    ReviewLevel = 1,
                    NextReviewDate = nowUtc.AddDays(1)
                };
                _context.UserVocabularyProgresses.Add(progress);
            }
            else
            {
                progress.IsLearned = true;
                progress.LearnedDate = nowUtc;
                progress.ReviewLevel = Math.Max(1, progress.ReviewLevel + 1);
                progress.NextReviewDate = nowUtc.AddDays(1);
                _context.UserVocabularyProgresses.Update(progress);
            }

            if (!wasLearned && plan.CompletedToday < plan.DailyTarget)
            {
                plan.CompletedToday++;
                _context.UserLearningPlans.Update(plan);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Phòng khi vẫn còn case FK khác
                return BadRequest(new { message = "Invalid vocabId (FK)", id = request.VocabId });
            }

            return NoContent(); // 204
        }


        /// <summary>
        /// Đánh dấu từ là chưa học
        /// POST /api/learning/mark-unlearned
        /// Body: { "vocabId": 123 }
        /// </summary>
        [HttpPost("mark-unlearned")]
        public async Task<IActionResult> MarkUnlearned([FromBody] VocabularyMarkRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var progress = await _context.UserVocabularyProgresses
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == request.VocabId);

            if (progress != null && progress.IsLearned)
            {
                progress.IsLearned = false;
                progress.LearnedDate = null;
                progress.ReviewLevel = 0;
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

        /// <summary>
        /// Lấy danh sách tất cả từ đã học
        /// GET /api/learning/learned-words
        /// </summary>
        [HttpGet("learned")]
        public async Task<IActionResult> GetLearned([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var query = _context.UserVocabularyProgresses
                .Where(p => p.UserId == user.Id && p.IsLearned)
                .Include(p => p.Vocabulary)
                .OrderBy(p => p.VocabularyId);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                total,
                items = items.Select(x => new {
                    x.VocabularyId,
                    vocabulary = new { x.Vocabulary.Id, x.Vocabulary.Tuvung, x.Vocabulary.Nghia },
                    x.ReviewLevel,
                    x.NextReviewDate,
                    x.LearnedDate
                })
            });
        }

    }
}
