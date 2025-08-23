using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using SharedModels;
using SharedModels.Models;
using doan.Repository;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ===== DbContext =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, x =>
    {
        x.MigrationsAssembly("doanapi"); // tên assembly chứa migrations
        x.MigrationsHistoryTable("__EFMigrationsHistory", "public"); // lưu history ở schema public
    })
);
// đổi đúng tên assembly migrations của API
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session, có thể thay đổi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== Identity =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })

.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===== DataProtection (CHỈ KHAI BÁO 1 LẦN) =====
// Lưu ý: MVC cũng phải cấu hình y hệt 2 dòng dưới
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("DoAnIdentity");

// ===== Cookie Auth (CHỈ KHAI BÁO 1 LẦN) =====
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.Name = ".AspNetCore.Identity.Application";
    opt.Cookie.SameSite = SameSiteMode.None;          // cần HTTPS
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opt.LoginPath = "/Account/Login";
    opt.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

// ===== CORS (CHỈ 1 POLICY) =====
// Thêm đúng origin của MVC (https và http nếu dùng cả hai)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcApp", policy =>
    {
        policy.WithOrigins("https://localhost:7080", "http://localhost:5266")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddRazorPages();

// ===== Repository =====
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// ===== Controllers =====
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("http://0.0.0.0:" + Environment.GetEnvironmentVariable("PORT"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.UseHttpsRedirection();


app.UseRouting();

// CORS phải nằm SAU UseRouting và TRƯỚC Auth
app.UseCors("AllowMvcApp");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
