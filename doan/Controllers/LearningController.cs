// File: doan/Controllers/LearningController.cs (FRONTEND CONTROLLER)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Models.DTO;
using SharedModels.Models.ViewModels;
using System.Net.Http;
using System.Net.Http.Json; // Cần thêm using này để dùng PostAsJsonAsync
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using SharedModels.Models;
[Authorize] // Yêu cầu người dùng phải đăng nhập để vào các trang học tập
public class LearningController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    // Thay thế port 7xxx bằng port của doanapi của bạn
    private readonly string _apiBaseUrl = "https://localhost:7191"; // ✅ sửa lại


    public LearningController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Action này xử lý trang học hàng ngày (/Learning/Daily)
    public async Task<IActionResult> Daily()
    {
        // Tạo một HTTP client đã được xác thực (nếu cần)
        // Cách làm này đơn giản, nhưng để an toàn hơn nên dùng token
        var client = _httpClientFactory.CreateClient();
        var apiUrl = $"{_apiBaseUrl}/api/learning/daily";

        // Gửi request GET đến API
        var response = await client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var viewModel = await JsonSerializer.DeserializeAsync<DailyLearningViewModel>(responseStream, options);

            return View(viewModel); // Truyền ViewModel vào View Daily.cshtml
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return Challenge(); // Chuyển hướng đến trang đăng nhập nếu chưa xác thực
        }

        ViewBag.ErrorMessage = "Không thể tải kế hoạch học tập.";
        return View("Error");
    }

    // Action này xử lý khi người dùng click "Đã học"
    // Nó sẽ được gọi bằng AJAX/Fetch từ phía client (JavaScript)
    [HttpPost]
    public async Task<IActionResult> MarkLearned([FromBody] VocabularyMarkRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var apiUrl = $"{_apiBaseUrl}/api/learning/mark-learned";

        // Gửi request POST với dữ liệu JSON đến API
        var response = await client.PostAsJsonAsync(apiUrl, request);

        if (response.IsSuccessStatusCode)
        {
            // Trả về 200 OK để JavaScript biết là đã thành công
            return Ok();
        }

        // Trả về mã lỗi để JavaScript có thể xử lý
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Action này xử lý khi người dùng click "Chưa học"
    [HttpPost]
    public async Task<IActionResult> MarkUnlearned([FromBody] VocabularyMarkRequest request)
    {

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
        var apiUrl = $"{_apiBaseUrl}/api/learning/mark-unlearned";

        var response = await client.PostAsJsonAsync(apiUrl, request);

        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }

        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Action này để hiển thị trang danh sách các từ đã học (/Learning/LearnedWords)
    public async Task<IActionResult> LearnedWords()
    {
        var client = _httpClientFactory.CreateClient();
        var apiUrl = $"{_apiBaseUrl}/api/learning/learned-words";

        var response = await client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            // API trả về IEnumerable<Vocabulary>, chúng ta deserialize nó thành List
            var learnedWords = await JsonSerializer.DeserializeAsync<List<Vocabulary>>(responseStream, options);

            return View(learnedWords); // Truyền danh sách từ đã học vào View LearnedWords.cshtml
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return Challenge();
        }

        ViewBag.ErrorMessage = "Không thể tải danh sách từ đã học.";
        return View("Error");
    }
}