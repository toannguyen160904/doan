using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using SharedModels.Models.DTO;
using SharedModels.Models.ViewModels;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace doan.Controllers
{
    [Authorize]
    public class LearningController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public LearningController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7191";
        }

        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);

            // Forward cookie từ MVC sang API
            var cookie = Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", cookie);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // GET /Learning/Daily
        public async Task<IActionResult> Daily(CancellationToken ct)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("/api/learning/daily", ct);

            if (response.IsSuccessStatusCode)
            {
                var vm = await response.Content.ReadFromJsonAsync<DailyLearningViewModel>(cancellationToken: ct);
                return View(vm ?? new DailyLearningViewModel());
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var returnUrl = Url.Action(nameof(Daily), "Learning");
                return RedirectToPage("/Account/Login", new { area = "Identity", ReturnUrl = returnUrl });
            }

            ViewBag.ErrorMessage = "Không thể tải kế hoạch học tập.";
            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkLearned([FromBody] VocabularyMarkRequest request, CancellationToken ct)
        {
            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("/api/learning/mark-learned", request, ct);

            if (response.IsSuccessStatusCode) return Ok();
            if (response.StatusCode == HttpStatusCode.Unauthorized) return Unauthorized();

            var body = await response.Content.ReadAsStringAsync(ct);
            return StatusCode((int)response.StatusCode, body);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkUnlearned(int VocabId)
        {
            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("/api/learning/mark-unlearned", new { VocabId });

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(LearnedWords));

            TempData["Error"] = "Không thể cập nhật trạng thái.";
            return RedirectToAction(nameof(LearnedWords));
        }
        public async Task<IActionResult> LearnedWords()
        {
            var client = CreateApiClient();
            var words = await client.GetFromJsonAsync<List<Vocabulary>>("/api/learning/learned-words");
            return View(words);
        }
    }
}
