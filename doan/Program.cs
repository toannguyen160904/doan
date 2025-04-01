using doan.Models;
using doan.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//  Đảm bảo chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("doan")
    ?? throw new InvalidOperationException("❌ Connection string 'doan' is missing. Kiểm tra appsettings.json!");

//  Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//  Cấu hình Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

})
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});
//  Đặt cấu hình Application Cookie SAU KHI AddDefaultIdentity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
//Đăng ký IVocabularyRepository service
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();


//  Thêm Razor Pages để hỗ trợ Identity UI
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
   

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());



app.UseAuthentication(); // Kích hoạt hệ thống đăng nhập

app.UseAuthorization();  // Kích hoạt phân quyền



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages(); //  Cần có để Identity UI hoạt động

app.Run();
