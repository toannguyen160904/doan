using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace doan.Helpers
{
    public class ApiHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApiHelper(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public HttpClient CreateHttpClientWithCookies()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false // ta sẽ forward cookie thủ công qua header
            };

            var cookie = _httpContextAccessor.HttpContext?.Request?.Headers["Cookie"].ToString();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"])
            };

            if (!string.IsNullOrEmpty(cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
            }

            return client;
        }
    }
}
