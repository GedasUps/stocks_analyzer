using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RentVest.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Security Check (Frontend level)
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Home"); // Kick them out
            }

            var token = HttpContext.Session.GetString("JWToken");
            var client = _httpClientFactory.CreateClient("GatewayClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync("gateway/admin/users");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(users);
                }
            }
            catch { }

            return View(new List<UserDto>());
        }
    }

    // Helper DTO for the view
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string RiskTolerance { get; set; }
    }
}