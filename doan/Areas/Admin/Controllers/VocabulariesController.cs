using SharedModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using SharedModels.Models;

namespace doan.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class VocabulariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VocabulariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Public Actions (Các phương thức chính)

        public async Task<IActionResult> Index()
        {
            var vocabularies = await _context.tuvung
                .Include(v => v.Lesson.Level) // Cách include gọn hơn
                .OrderBy(v => v.Lesson != null ? v.Lesson.LevelId : 0)
                .ThenBy(v => v.LessonId)
                .ToListAsync();

            await PopulateLevelsViewBag();
            return View(vocabularies);
        }

        public IActionResult Create()
        {
            PopulateLevelsViewBag().Wait(); // Dùng .Wait() cho phương thức không async
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vocabulary vocab)
        {
            if (await IsDuplicateVocabularyAsync(vocab))
            {
                ModelState.AddModelError("HanTu", $"Từ vựng với Hán Tự '{vocab.HanTu}' đã tồn tại trong bài học này.");
            }

            if (ModelState.IsValid)
            {
                _context.tuvung.Add(vocab);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm từ vựng mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsForViewAsync(vocab);
            return View(vocab);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vocab = await _context.tuvung.FindAsync(id);
            if (vocab == null) return NotFound();

            await PopulateDropdownsForViewAsync(vocab);
            return View(vocab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vocabulary vocabFromForm)
        {
            if (id != vocabFromForm.Id) return NotFound();

            var vocabToUpdate = await _context.tuvung.FindAsync(id);
            if (vocabToUpdate == null) return NotFound();

            if (await IsDuplicateVocabularyAsync(vocabFromForm))
            {
                ModelState.AddModelError("HanTu", $"Từ vựng với Hán Tự '{vocabFromForm.HanTu}' đã tồn tại trong bài học này.");
            }

            if (ModelState.IsValid)
            {
                // Áp dụng mẫu "Read-Modify-Save"
                vocabToUpdate.Tuvung = vocabFromForm.Tuvung;
                vocabToUpdate.PhatAm = vocabFromForm.PhatAm;
                vocabToUpdate.AmHan = vocabFromForm.AmHan;
                vocabToUpdate.HanTu = vocabFromForm.HanTu;
                vocabToUpdate.Nghia = vocabFromForm.Nghia;
                if (vocabFromForm.LessonId > 0)
                {
                    vocabToUpdate.LessonId = vocabFromForm.LessonId;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật từ vựng thành công!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsForViewAsync(vocabFromForm);
            return View(vocabToUpdate); // Trả về đối tượng đã load từ DB
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.tuvung.Include(v => v.Lesson).FirstOrDefaultAsync(v => v.Id == id);
            if (vocab == null) return NotFound();
            return View(vocab);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vocab = await _context.tuvung.FindAsync(id);
            if (vocab != null)
            {
                _context.tuvung.Remove(vocab);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa từ vựng thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region API and Partial Views (Các phương thức phụ trợ)

        [HttpGet]
        public async Task<IActionResult> _VocabularyListPartial(int? levelId, int? lessonId)
        {
            var query = _context.tuvung
                .Include(v => v.Lesson.Level)
                .AsQueryable();

            if (lessonId.HasValue && lessonId > 0)
            {
                query = query.Where(v => v.LessonId == lessonId.Value);
            }
            else if (levelId.HasValue && levelId > 0)
            {
                query = query.Where(v => v.Lesson != null && v.Lesson.LevelId == levelId.Value);
            }

            var result = await query
                .OrderBy(v => v.Lesson != null ? v.Lesson.LevelId : 0)
                .ThenBy(v => v.LessonId)
                .ToListAsync();

            return PartialView("_VocabularyListPartial", result);
        }

        [HttpGet("/Admin/Vocabularies/GetLessonsByLevel/{levelId}")]
        public async Task<IActionResult> GetLessonsByLevel(int levelId)
        {
            var lessons = await _context.Baihoc
                .Where(b => b.LevelId == levelId && !b.IsDeleted)
                .Select(b => new { id = b.Id, name = b.Name })
                .ToListAsync();

            return Ok(lessons);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.tuvung.Include(v => v.Lesson).FirstOrDefaultAsync(m => m.Id == id);
            if (vocab == null) return NotFound();
            return View(vocab);
        }

        #endregion

        #region Private Helper Methods (Các phương thức nội bộ)

        /// <summary>
        /// Kiểm tra xem một từ vựng có bị trùng lặp (dựa trên HanTu và LessonId) hay không.
        /// </summary>
        private async Task<bool> IsDuplicateVocabularyAsync(Vocabulary vocab)
        {
            if (string.IsNullOrEmpty(vocab.HanTu) || vocab.LessonId <= 0)
            {
                return false;
            }

            // Nếu vocab.Id > 0, nghĩa là đang edit, cần loại trừ chính nó ra khỏi việc kiểm tra.
            return await _context.tuvung.AnyAsync(v =>
                v.HanTu.ToLower() == vocab.HanTu.ToLower() &&
                v.LessonId == vocab.LessonId &&
                v.Id != vocab.Id
            );
        }

        /// <summary>
        /// Chuẩn bị dữ liệu cho các Dropdown (Level, Lesson) để hiển thị trên View.
        /// </summary>
        private async Task PopulateDropdownsForViewAsync(Vocabulary vocab)
        {
            var lesson = await _context.Baihoc.FindAsync(vocab.LessonId);
            int? levelId = lesson?.LevelId;

            await PopulateLevelsViewBag(levelId);
            await PopulateLessonsViewBag(levelId, vocab.LessonId);
        }

        /// <summary>
        /// Chuẩn bị ViewBag cho dropdown Levels.
        /// </summary>
        private async Task PopulateLevelsViewBag(object selectedLevel = null)
        {
            // === THÊM .OrderBy(l => l.Id) VÀO ĐÂY ===
            var levels = await _context.Levels
                                     .OrderBy(l => l.Id) // Sắp xếp theo ID tăng dần
                                     .ToListAsync();

            ViewBag.Levels = new SelectList(levels, "Id", "Name", selectedLevel);
        }

        /// <summary>
        /// Chuẩn bị ViewBag cho dropdown Lessons.
        /// </summary>
        private async Task PopulateLessonsViewBag(int? levelId, object selectedLesson = null)
        {
            if (levelId.HasValue)
            {
                ViewBag.Lessons = new SelectList(
                    await _context.Baihoc.Where(b => b.LevelId == levelId.Value && !b.IsDeleted).ToListAsync(),
                    "Id", "Name", selectedLesson);
            }
            else
            {
                ViewBag.Lessons = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        #endregion
    }
}