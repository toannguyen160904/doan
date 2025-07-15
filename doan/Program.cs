// File: doan/Program.cs (ĐÃ SẮP XẾP LẠI CHO ĐÚNG)

using SharedModels;
using doan.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

var builder = WebApplication.CreateBuilder(args);
// ===================================================================
// == PHẦN ĐĂNG KÝ DỊCH VỤ (Tất cả phải nằm trước builder.Build()) ==
// ===================================================================

// 🔌 Kết nối cơ sở dữ liệu
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("❌ Connection string 'doan' is missing.");

// SỬA "doan" thành "DataAccess" nếu bạn đã di chuyển thư mục Migrations
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, x =>
        x.MigrationsAssembly("doan")));

// 🔐 Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // ... options của bạn ...
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// 🍪 Cấu hình cookie xác thực
builder.Services.ConfigureApplicationCookie(options =>
{
    // ... options của bạn ...
});

// 🧠 Đăng ký các Repository (Bạn muốn giữ lại)
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 📞 Đăng ký HttpClient để gọi API (Đúng vị trí)
builder.Services.AddHttpClient();

// 🕒 Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // ... options của bạn ...
});

// 📄 Dịch vụ cho MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// ===================================================================
// == DÒNG CHỐT SỔ - XÂY DỰNG ỨNG DỤNG ==
// ===================================================================
var app = builder.Build();


// ===================================================================
// == PHẦN CẤU HÌNH MIDDLEWARE (Tất cả nằm sau builder.Build()) ==
// ===================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🌍 Bỏ CORS - Frontend không cần
// app.UseCors(...); 

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// ✅ Route cho Areas và Default
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Razor Pages cho Identity UI
app.MapRazorPages();

// ❌ Bỏ MapControllers() - Không cần thiết cho MVC
// app.MapControllers(); 

app.Run();