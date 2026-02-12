using Microsoft.AspNetCore.Mvc;
using RentVest.Web.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt; // Required for decoding the token
using System.Linq;

namespace RentVest.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SimulationViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(SimulationViewModel model)
        {
            // 1. Retrieve Token
            var token = HttpContext.Session.GetString("JWToken");
            int extractedUserId = 0;

            // 2. TOKEN DECODING WORKAROUND
            // Since Ocelot might strip headers, we extract the ID here and send it in the body.
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    // We look for standard Claim types for User ID
                    // "nameid" is standard JWT, the URL is standard .NET Identity
                    var idClaim = jwtToken.Claims.FirstOrDefault(c =>
                        c.Type == "nameid" ||
                        c.Type == "sub" ||
                        c.Type == "id" ||
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                    if (idClaim != null)
                    {
                        if (int.TryParse(idClaim.Value, out int parsedId))
                        {
                            extractedUserId = parsedId;
                            Console.WriteLine($"[Frontend] Successfully extracted UserID: {extractedUserId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Frontend] Token decoding error: {ex.Message}");
                }
            }
            else
            {
                model.ErrorMessage = "You must be logged in to run a simulation.";
                return View(model);
            }

            // 3. Prepare Payload (Including userId)
            var requestPayload = new
            {
                investableAmount = model.RentAmount,
                currency = model.Currency,
                investmentDate = model.StartDate,
                riskTolerance = model.RiskLevel,
                userId = extractedUserId // <--- SENDING ID MANUALLY
            };

            try
            {
                var client = _httpClientFactory.CreateClient("GatewayClient");

                // We still attach the header (Best Practice), even if Gateway strips it
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestPayload),
                    Encoding.UTF8,
                    "application/json");

                // 4. Call Gateway
                var response = await client.PostAsync("gateway/recommendation/generate", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // Deserialize result
                    model.Results = JsonSerializer.Deserialize<SimulationResult>(responseString, options);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    model.ErrorMessage = $"API Error ({response.StatusCode}): {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Connection failed: {ex.Message}. Is Gateway running?";
            }

            return View(model);
        }
    }
}