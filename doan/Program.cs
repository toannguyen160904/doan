using doan.Models;
using doan.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔌 Kết nối cơ sở dữ liệu
var connectionString = builder.Configuration.GetConnectionString("doan")
    ?? throw new InvalidOperationException("❌ Connection string 'doan' is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🔐 Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // UI mặc định của Identity

// 🍪 Cấu hình cookie xác thực
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// 🧠 Đăng ký các Repository
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();



builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Đặt thời gian timeout cho session, ví dụ 30 phút.
    // Sau 30 phút không có request nào, session sẽ hết hạn.
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đánh dấu cookie session là cần thiết
});
// 📄 Razor Pages + Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 🛠 Middleware xử lý pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🌍 CORS nếu cần (bạn có thể bỏ nếu không sử dụng)
app.UseCors(cors =>
    cors.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

// ✅ Identity middlewares
app.UseAuthentication();
app.UseAuthorization();


// ✅ Route cho Areas (rất quan trọng)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// ✅ Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSession();

// ✅ Razor Pages cho Identity UI
app.MapRazorPages();

app.MapControllers(); // Cho phép route API hoạt động


app.Run();
