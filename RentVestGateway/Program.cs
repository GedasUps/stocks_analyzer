using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURATION
// Load the routing rules
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
var secretKey = builder.Configuration.GetValue<string>("AppSettings:Token");

// 2. AUTHENTICATION
// The Gateway acts as a "Bouncer". It checks the ID card before letting the request in.
// IMPORTANT: This Key must match the API Key exactly.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// 3. OCELOT SERVICES
builder.Services.AddOcelot();

var app = builder.Build();

// 4. MIDDLEWARE PIPELINE
app.UseHttpsRedirection();

app.UseAuthentication(); // Check Token
app.UseAuthorization();  // Check Permissions

// Activate Ocelot to handle the routing
await app.UseOcelot();

app.Run();