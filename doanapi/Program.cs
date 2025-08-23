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
        x.MigrationsAssembly("doanapi");
        x.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    })
);

// ===== Session =====
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
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

// ===== DataProtection =====
if (!builder.Environment.IsProduction())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"C:\SharedKeys"))
        .SetApplicationName("DoAnIdentity");
}
else
{
    builder.Services.AddDataProtection()
        .SetApplicationName("DoAnIdentity");
}

// ===== Cookie Auth =====
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.Name = ".AspNetCore.Identity.Application";
    opt.Cookie.SameSite = SameSiteMode.None;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opt.LoginPath = "/Account/Login";
    opt.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; },
        OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; }
    };
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcApp", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7080",
                "http://localhost:5266",
                // thêm domain MVC thật ở production nếu có, ví dụ:
                "https://keen-transformation-production.up.railway.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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

// Swagger: bật ở Dev hoặc khi đặt biến môi trường ENABLE_SWAGGER=true
if (app.Environment.IsDevelopment() ||
    string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Thứ tự đúng
app.UseRouting();

app.UseCors("AllowMvcApp");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Chỉ cần cho API:
app.MapControllers();

// (tuỳ chọn) root check
app.MapGet("/", () => Results.Ok("doanapi is running"));

app.Run();
