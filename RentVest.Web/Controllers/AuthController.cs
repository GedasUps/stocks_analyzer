using Microsoft.AspNetCore.Mvc;
using RentVest.Web.Models;
using System.Text;
using System.Text.Json;

namespace RentVest.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("GatewayClient");

                // Prepare Payload (Matches your API UserDto)
                var registerPayload = new
                {
                    username = model.Username,
                    password = model.Password
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(registerPayload),
                    Encoding.UTF8,
                    "application/json");

                // Call Gateway Route (defined in ocelot.json)
                var response = await client.PostAsync("gateway/users/register", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Success! Redirect to Login or Home
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Login"); // You will create Login later
                }
                else
                {
                    // Read error from API (e.g., "Username already taken")
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Registration failed: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Connection error: {ex.Message}");
            }

            // If we got here, something failed, redisplay form
            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("GatewayClient");

                var loginPayload = new { username = model.Username, password = model.Password };
                var jsonContent = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");

                // Call Gateway Route
                var response = await client.PostAsync("gateway/users/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Parse the JSON to get the Token
                    // Assuming API returns: { "token": "eyJh..." }
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        if (doc.RootElement.TryGetProperty("token", out JsonElement tokenElement))
                        {
                            string token = tokenElement.GetString();
                            HttpContext.Session.SetString("JWToken", token);

                            // --- NEW: EXTRACT ROLE ---
                            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                            var jwtToken = handler.ReadJwtToken(token);

                            // Find the Role Claim
                            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                            if (roleClaim != null)
                            {
                                HttpContext.Session.SetString("UserRole", roleClaim.Value);
                            }
                            // -------------------------

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"System error: {ex.Message}");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
