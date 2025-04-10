using doan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class BaihocApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BaihocApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("GetByLevel/{levelId}")]
    public async Task<IActionResult> GetByLevel(int levelId)
    {
        var lessons = await _context.Baihoc
            .Where(b => b.LevelId == levelId)
            .Select(b => new {
                id = b.Id,      
                name = b.Name   
            })
            .ToListAsync();

        return Ok(lessons);
    }

}
