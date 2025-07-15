// File: doanapi/Controllers/TuDienController.cs (?Ã CH?NH S?A)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models; // Namespace ch?a các model
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Thêm các attribute chu?n
[ApiController]
[Route("api/[controller]")]
public class TuDienController : ControllerBase // 2. K? th?a t? ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TuDienController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 3. Chuy?n ??i Action 'Index' thành m?t endpoint tìm ki?m
    // Endpoint này s? nh?n tham s? qua query string
    // Ví d? URL: /api/tudien/search?searchTerm=neko&filter=TuVung
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Vocabulary>>> Search(
        [FromQuery] string searchTerm,
        [FromQuery] string filter = "TuVung")
    {
        // Hi?n t?i ch? h? tr? tìm ki?m t? v?ng
        if (filter != "TuVung")
        {
            // Tr? v? m?t danh sách r?ng n?u filter không ???c h? tr?
            return Ok(new List<Vocabulary>());
        }

        // B?t ??u câu truy v?n
        var query = _context.tuvung.AsQueryable();

        // Áp d?ng b? l?c tìm ki?m n?u có searchTerm
        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Chu?n hóa chu?i tìm ki?m
            var normalizedSearchTerm = searchTerm.ToLower().Trim();

            query = query.Where(v =>
                (v.Tuvung != null && v.Tuvung.ToLower().Contains(normalizedSearchTerm)) ||
                (v.Nghia != null && v.Nghia.ToLower().Contains(normalizedSearchTerm)) ||
                (v.HanTu != null && v.HanTu.ToLower().Contains(normalizedSearchTerm)) ||
                (v.PhatAm != null && v.PhatAm.ToLower().Contains(normalizedSearchTerm))
            );
        }

        // S?p x?p k?t qu? ?? ??m b?o th? t? nh?t quán
        query = query.OrderBy(v => v.Tuvung);

        // L?y k?t qu? t? database
        var results = await query.ToListAsync();

        // 4. Tr? v? k?t qu? tr?c ti?p d??i d?ng JSON, không c?n ViewBag
        return Ok(results);
    }
}