using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using SharedModels;
using SharedModels.Models;
using doan.Helpers;


var builder = WebApplication.CreateBuilder(args);

// ===== DbContext & Identity =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, x =>
    {
        x.MigrationsAssembly("doanapi");                      // hoặc assembly chứa migrations thật sự
        x.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    })
);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session, có thể thay đổi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ===== DataProtection dùng chung key để share cookie =====
if (!builder.Environment.IsProduction())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"C:\SharedKeys")) // Local
        .SetApplicationName("DoAnIdentity");
}
else
{
    builder.Services.AddDataProtection()
        .SetApplicationName("DoAnIdentity"); // Railway
}
// ===== Cookie auth (chỉ gọi 1 lần) =====
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.Name = ".AspNetCore.Identity.Application";
    opt.Cookie.SameSite = SameSiteMode.None;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opt.LoginPath = "/Identity/Account/Login";
    opt.LogoutPath = "/Identity/Account/Logout";
    opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
    opt.SlidingExpiration = true;
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
});
builder.Services.AddRazorPages();

// ===== HttpClient gọi Web API =====
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!); 
});
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

// 1) Thêm Google vào AuthenticationBuilder
authBuilder.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    googleOptions.SaveTokens = true;

    // luôn dùng đúng callback này
    googleOptions.CallbackPath = "/signin-google";

    // (tuỳ chọn) ép hiện màn hình chọn tài khoản + in ra URL để bạn kiểm tra
    googleOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        var redirect = context.RedirectUri;

        // 🛠 Bắt buộc chuyển sang HTTPS (Railway dùng proxy HTTPS)
        redirect = redirect.Replace("http://", "https://");

        // Optional: yêu cầu chọn tài khoản Google mỗi lần
        redirect += "&prompt=select_account";

        Console.WriteLine(">>> GOOGLE RedirectUri SENT: " + redirect);

        context.Response.Redirect(redirect);
        return Task.CompletedTask;
    };

});
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("doan-system");
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApiHelper>();

builder.Services.AddControllersWithViews();
builder.WebHost.UseUrls("http://0.0.0.0:" + Environment.GetEnvironmentVariable("PORT"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
