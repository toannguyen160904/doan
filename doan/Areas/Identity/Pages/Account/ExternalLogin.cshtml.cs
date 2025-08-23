using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

        // POST -> gửi sang Google
        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // Callback từ Google
        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null, string? flow = "login")
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

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                ErrorMessage = "Google không cung cấp email.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
            var user = await _userManager.FindByEmailAsync(email);

            if (flow == "register")
            {
                // Đăng ký
                if (user != null)
                {
                    // ❌ Email đã tồn tại
                    ErrorMessage = "Tài khoản này đã tồn tại. Vui lòng đăng nhập.";
                    return RedirectToPage("./Register");
                }

                // Tạo mới
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Name = fullName
                };

                var createRes = await _userManager.CreateAsync(user);
                if (!createRes.Succeeded)
                {
                    ErrorMessage = string.Join("; ", createRes.Errors.Select(e => e.Description));
                    return RedirectToPage("./Register");
                }

                var addLoginRes = await _userManager.AddLoginAsync(user, info);
                if (!addLoginRes.Succeeded)
                {
                    ErrorMessage = string.Join("; ", addLoginRes.Errors.Select(e => e.Description));
                    return RedirectToPage("./Register");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(SafeReturnUrl(returnUrl, User, isAdmin: false));
            }
            else
            {
                // flow == login
                if (user == null)
                {
                    ErrorMessage = "Tài khoản chưa được đăng ký. Vui lòng đăng ký trước.";
                    return RedirectToPage("./Login");
                }

                // Liên kết nếu chưa có
                var addLoginRes = await _userManager.AddLoginAsync(user, info);
                if (!addLoginRes.Succeeded && addLoginRes.Errors.Any(e => e.Code != "LoginAlreadyAssociated"))
                {
                    ErrorMessage = string.Join("; ", addLoginRes.Errors.Select(e => e.Description));
                    return RedirectToPage("./Login");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(SafeReturnUrl(returnUrl, User, isAdmin: await _userManager.IsInRoleAsync(user, "Admin")));
            }
        }


        /// <summary>
        /// Chỉ cho phép quay lại URL nội bộ; chặn /Logout, /Lockout; nếu không phải admin thì chặn /Admin.
        /// </summary>
        private string SafeReturnUrl(string? returnUrl, ClaimsPrincipal principal, bool isAdmin)
        {
            var safe = (Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/")) ?? Url.Content("~/");

            string[] blocked = new[]
            {
                "/Identity/Account/Logout",
                "/Identity/Account/Lockout"
            };
            foreach (var b in blocked)
                if (safe.StartsWith(b, StringComparison.OrdinalIgnoreCase))
                    safe = Url.Content("~/");

            if (!isAdmin && safe.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                safe = Url.Content("~/");

            return safe;
        }
    }
}
