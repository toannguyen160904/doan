using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedModels.Models;

namespace doan.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ProviderDisplayName { get; set; }
        public string? ReturnUrl { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        // POST /ExternalLogin -> chuyển sang Google
        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // GET /ExternalLogin?handler=Callback -> Google gọi lại
        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Lỗi đăng nhập ngoài: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Không lấy được thông tin đăng nhập ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Đăng nhập nếu user đã có liên kết
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                // ⭐ Reset lockout cho chắc (trường hợp user từng bị lock do login local sai)
                var emailClaim = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(emailClaim))
                {
                    var u = await _userManager.FindByEmailAsync(emailClaim);
                    if (u != null)
                    {
                        await _userManager.SetLockoutEndDateAsync(u, null);
                        await _userManager.ResetAccessFailedCountAsync(u);
                        await _userManager.SetLockoutEnabledAsync(u, false);
                    }
                }

                // Bảo vệ returnUrl & tránh trả user thường vào /Admin
                return LocalRedirect(SafeReturnUrl(returnUrl, User, isAdmin: false));
            }

            // Nếu chưa có -> tạo theo email + gán Name
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                ErrorMessage = "Google không cung cấp email.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var fullName =
                info.Principal.FindFirstValue(ClaimTypes.Name) ??
                info.Principal.FindFirstValue("name") ??
                email; // fallback

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Name = fullName,  // tránh NULL cột Name
                };

                var createRes = await _userManager.CreateAsync(user);
                if (!createRes.Succeeded)
                {
                    ErrorMessage = string.Join("; ", createRes.Errors.Select(e => e.Description));
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }
            else
            {
                // Nếu user đã tồn tại nhưng Name còn trống (phòng hờ)
                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    user.Name = fullName;
                    await _userManager.UpdateAsync(user);
                }
            }

            // Liên kết login ngoài
            var addLoginRes = await _userManager.AddLoginAsync(user, info);
            if (!addLoginRes.Succeeded)
            {
                ErrorMessage = string.Join("; ", addLoginRes.Errors.Select(e => e.Description));
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // ⭐ Reset lockout trước khi sign-in để không bị chuyển tới trang Lockout
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEnabledAsync(user, false);

            // Đăng nhập
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Nếu bạn muốn chặn user thường vào /Admin khi returnUrl trỏ vào đó:
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            return LocalRedirect(SafeReturnUrl(returnUrl, User, isAdmin));
        }

        /// <summary>
        /// Chỉ cho phép quay lại URL nội bộ; nếu user không phải admin và URL trỏ vào /Admin thì trả về "/".
        /// </summary>
        // ... using như bạn có ở trên

        private string SafeReturnUrl(string? returnUrl, ClaimsPrincipal principal, bool isAdmin)
        {
            var safe = (Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/")) ?? Url.Content("~/");

            // Chặn returnUrl không hợp lệ
            string[] blocked = new[]
            {
        "/Identity/Account/Logout",
        "/Identity/Account/Lockout"
    };

            foreach (var b in blocked)
                if (safe.StartsWith(b, StringComparison.OrdinalIgnoreCase))
                    safe = Url.Content("~/");

            // Không cho user thường vào /Admin
            if (!isAdmin && safe.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                safe = Url.Content("~/");

            return safe;
        }

    }
}
