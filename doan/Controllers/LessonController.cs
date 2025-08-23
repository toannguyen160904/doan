using Microsoft.AspNetCore.Mvc;
using SharedModels.Models.ViewModels;
using SharedModels.Models.DTO;
using System.Text.Json;
using System.Net;
using Microsoft.AspNetCore.Authorization;

public class LessonController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiBaseUrl ; // Cập nhật đúng port API của bạn
    private object configuration;

    public LessonController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("Thiếu cấu hình ApiSettings:BaseUrl");
    }
    [HttpGet]
    public async Task<IActionResult> VocabularyPartial(int id, int page)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());

        var apiUrl = $"{_apiBaseUrl}/api/lesson/{id}?page={page}";
        var response = await client.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            return Content("Không thể tải từ vựng.");
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var viewModel = JsonSerializer.Deserialize<LessonDetailsViewModel>(jsonString, options);

        return PartialView("_VocabularyPartial", viewModel);
    }


    // Chi tiết bài học
    public async Task<IActionResult> Details(int id, int page = 1, int commentPage = 1)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());

        var url = $"{_apiBaseUrl}/api/lesson/{id}?page={page}&commentPage={commentPage}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = $"Lỗi API: {response.StatusCode}";
            return View("Error");
        }

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var viewModel = JsonSerializer.Deserialize<LessonDetailsViewModel>(json, options);

        return View("~/Views/level/Details.cshtml", viewModel);
    }

    // Đăng bình luận diễn đàn
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DangBai([FromBody] CreatePostDto postData)
    {
        var handler = new HttpClientHandler { UseCookies = true, CookieContainer = new CookieContainer() };
        foreach (var cookie in Request.Cookies)
        {
            handler.CookieContainer.Add(new Uri(_apiBaseUrl), new Cookie(cookie.Key, cookie.Value));
        }

        var client = new HttpClient(handler) { BaseAddress = new Uri(_apiBaseUrl) };
        var response = await client.PostAsJsonAsync("api/lesson/post", postData);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return Json(new { success = false, message = err });
        }

        var post = await response.Content.ReadFromJsonAsync<PostViewModel>();
        return Json(new { success = true, post });
    }
}
