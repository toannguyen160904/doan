using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using doan.Models;  // 

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hiển thị danh sách từ vựng
    

    // Hiển thị form tạo từ vựng
    public IActionResult Create()
    {
        return View();
    }

    // Xử lý khi submit form tạo từ vựng
 
}
