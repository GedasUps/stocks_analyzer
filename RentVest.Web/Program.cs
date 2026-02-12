var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES
builder.Services.AddControllersWithViews();

// Setup Session to store the JWT Token after login
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Setup HttpClient to talk to the Gateway
builder.Services.AddHttpClient("GatewayClient", client =>
{
    // The address of your Gateway Project
    client.BaseAddress = new Uri("https://localhost:7149/");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    // Bypass SSL errors for localhost development (Self-signed certs)
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
});

// ADD THIS LINE HERE:
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 2. MIDDLEWARE PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enables loading CSS, JS, Images from wwwroot
app.UseStaticFiles();

app.UseRouting();

// Enables accessing HttpContext.Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();