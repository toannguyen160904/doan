// File: Areas/Admin/Controllers/ForumController.cs

using doan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Area("Admin")] // Quan trọng: Khai báo Controller này thuộc Area "Admin"
[Authorize(Roles = "Admin")] // Chỉ người dùng có vai trò "Admin" mới được truy cập
public class ForumController : Controller
{
    private readonly ApplicationDbContext _context;

    public ForumController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Admin/Forum/Index
    // Trang chính để quản lý tất cả bài viết
    public async Task<IActionResult> Index()
    {
        var allPosts = await _context.Diendan
            .Include(p => p.User)
            .Include(p => p.Baihoc)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View("~/Areas/Admin/Views/diendan/Index.cshtml", allPosts);
    }

    // POST: /Admin/Forum/DeletePost/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id, string returnUrl = null)
    {
        var post = await _context.Diendan.FindAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        _context.Diendan.Remove(post);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Bài viết đã được xóa thành công!";

        // Nếu có URL để quay lại, thì quay lại đó
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // Mặc định, quay về trang quản lý
        return RedirectToAction(nameof(Index));
    }
}