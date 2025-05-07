using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using doan.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;
using doan.Controllers;

[Authorize]
public class LearningController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LearningController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Daily()
    {
        var user = await _userManager.GetUserAsync(User);
        var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);

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

        var vocabList = await _context.tuvung
            .Where(v => !learnedIds.Contains(v.Id))
            .OrderBy(v => Guid.NewGuid())
            .Take(10)
            .ToListAsync();

        foreach (var vocab in vocabList)
        {
            var progress = await _context.UserVocabularyProgresses
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == vocab.Id);

            if (progress != null)
            {
                vocab.CreatedAt = progress.Vocabulary.CreatedAt;
            }
        }

        // Từ cần ôn lại hôm nay
        var reviewList = await _context.UserVocabularyProgresses
            .Where(p => p.UserId == user.Id && p.IsLearned && p.NextReviewDate <= DateTime.Today)
            .Include(p => p.Vocabulary)
            .Select(p => p.Vocabulary)
            .ToListAsync();

        plan.VocabularyList = vocabList;

        ViewData["CompletedToday"] = plan.CompletedToday;
        ViewData["DailyTarget"] = plan.DailyTarget;
        ViewBag.LearnedIds = learnedIds;
        ViewBag.ReviewList = reviewList;

        return View(plan);
    }

    [HttpPost]
    public async Task<IActionResult> MarkLearned(int VocabId)
    {
        var user = await _userManager.GetUserAsync(User);
        var progress = await _context.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == VocabId);

        var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (progress == null)
        {
            progress = new UserVocabularyProgress
            {
                UserId = user.Id,
                VocabularyId = VocabId,
                IsLearned = true,
                LearnedDate = DateTime.Now,
                ReviewLevel = 1,
                NextReviewDate = DateTime.Now.AddDays(1)
            };
            _context.UserVocabularyProgresses.Add(progress);

            if (plan != null && plan.CompletedToday < plan.DailyTarget)
            {
                plan.CompletedToday++;
                _context.UserLearningPlans.Update(plan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Daily");
        }

        if (!progress.IsLearned)
        {
            progress.IsLearned = true;
            progress.LearnedDate = DateTime.Now;

            switch (progress.ReviewLevel)
            {
                case 0: progress.NextReviewDate = DateTime.Now.AddDays(1); break;
                case 1: progress.NextReviewDate = DateTime.Now.AddDays(3); break;
                case 2: progress.NextReviewDate = DateTime.Now.AddDays(7); break;
                default: progress.NextReviewDate = DateTime.Now.AddDays(14); break;
            }
            progress.ReviewLevel++;

            if (plan != null && plan.CompletedToday < plan.DailyTarget)
            {
                plan.CompletedToday++;
                _context.UserLearningPlans.Update(plan);
            }

            _context.UserVocabularyProgresses.Update(progress);
            await _context.SaveChangesAsync();

            return RedirectToAction("Daily");
        }

        return RedirectToAction("Daily");
    }

    [HttpPost]
    public async Task<IActionResult> MarkUnlearned(int VocabId)
    {
        var user = await _userManager.GetUserAsync(User);
        var progress = await _context.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VocabularyId == VocabId);

        if (progress == null)
        {
            return RedirectToAction("Daily");
        }

        if (progress.IsLearned)
        {
            progress.IsLearned = false;
            progress.LearnedDate = null;
            progress.ReviewLevel = 0;
            progress.NextReviewDate = null;

            var plan = await _context.UserLearningPlans.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (plan != null && plan.CompletedToday > 0)
            {
                plan.CompletedToday--;
                _context.UserLearningPlans.Update(plan);
            }

            _context.UserVocabularyProgresses.Update(progress);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Daily");
    }

    [HttpGet]
    public async Task<IActionResult> LearnedWords()
    {
        var user = await _userManager.GetUserAsync(User);

        var learnedWords = await _context.UserVocabularyProgresses
            .Where(p => p.UserId == user.Id && p.IsLearned)
            .Include(p => p.Vocabulary)
            .Select(p => p.Vocabulary)
            .ToListAsync();

        return View(learnedWords);
    }

    public class VocabularyMarkRequest
    {
        public int VocabId { get; set; }
    }
}
