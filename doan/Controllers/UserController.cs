using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using SharedModels.Models;

namespace doan.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 Hiển thị danh sách sản phẩm (Chỉ xem, không sửa, xóa)
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }
    }
}
