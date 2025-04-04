using doan.Models;
using Microsoft.AspNetCore.Mvc;

namespace doan.Controllers
{
    [Route("api/lessons")]
    [ApiController]
    public class LessonsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LessonsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-level/{levelId}")]
        public IActionResult GetLessonsByLevel(int levelId)
        {
            var lessons = _context.Baihoc
                .Where(l => l.LevelId == levelId)
                .Select(l => new
                {
                    id = l.Id,
                    name = l.Name
                }).ToList();

            return Ok(lessons);
        }
    }
}
