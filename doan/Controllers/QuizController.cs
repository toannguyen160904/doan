using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan.Models;
using System.Linq;
using System.Threading.Tasks;

namespace doan.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action để hiển thị trang làm bài Quiz
        public IActionResult Quiz(int baiHocId)
        {
            // Lấy Quiz dựa trên bài học
            var quiz = _context.Quizzes
                .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
                .FirstOrDefault(q => q.BaihocId == baiHocId);

            if (quiz == null)
            {
                return NotFound("Quiz không tồn tại cho bài học này.");
            }

            return View(quiz);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChamDiem(int quizId, Dictionary<int, int> userAnswers)
        {
            var quiz = _context.Quizzes
                .Include(q => q.CauHois)
                .ThenInclude(ch => ch.CauTraLois)
                .FirstOrDefault(q => q.Id == quizId);

            if (quiz == null)
                return Json(new { error = "Quiz không tồn tại." });

            int soCauDung = 0;
            int tongSoCau = quiz.CauHois.Count;

            foreach (var cauHoi in quiz.CauHois)
            {
                if (userAnswers.TryGetValue(cauHoi.Id, out int selectedAnswerId))
                {
                    var dapAn = cauHoi.CauTraLois.FirstOrDefault(ct => ct.Id == selectedAnswerId);
                    if (dapAn != null && dapAn.IsCorrect)
                    {
                        soCauDung++;
                    }
                }
                ViewBag.SoCauDung = soCauDung;
                ViewBag.TongSoCau = tongSoCau;
            }

            return View("~/Views/Home/Quiz.cshtml", quiz);
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ChamDiem(int quizId, Dictionary<int, int> userAnswers)
        //{
        //    var quiz = _context.Quizzes
        //        .Include(q => q.CauHois)
        //            .ThenInclude(ch => ch.CauTraLois)
        //        .FirstOrDefault(q => q.Id == quizId);

        //    int soCauDung = 0;
        //    int tongSoCau = quiz.CauHois.Count;

        //    foreach (var cauHoi in quiz.CauHois)
        //    {
        //        if (userAnswers.TryGetValue(cauHoi.Id, out int selectedAnswerId))
        //        {
        //            var dapAn = cauHoi.CauTraLois.FirstOrDefault(ct => ct.Id == selectedAnswerId);
        //            if (dapAn != null && dapAn.IsCorrect)
        //            {
        //                soCauDung++;
        //            }
        //        }
        //    }

        //    return Json(new { soCauDung, tongSoCau });
        //}

    }
}
