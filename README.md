# 📈 RentVest - Investment Simulation System


**RentVest** is an engineering thesis project designed to simulate historical investment returns. It addresses the "Rent vs. Invest" dilemma by allowing users to calculate potential profits if they had invested their monthly rent money into a diversified portfolio (Stocks, ETFs, Bonds) over a specific historical period.

The system utilizes a **Layered Microservices-like Architecture** with an **API Gateway** to orchestrate traffic between the Client and Backend, integrating real-time financial data from multiple external providers.

---

## 🏗️ Architecture

The solution follows a distributed architecture design ensuring separation of concerns, security, and scalability.

```
mermaid
graph LR
    User[User / Browser] -- HTTPS --> Web[RentVest.Web (MVC)]
    Web -- HTTPS/JSON --> Gateway[Ocelot Gateway]
    Gateway -- HTTP --> API[RentVest.Api]
    API -- TCP/IP --> DB[(Database)]
    API -- JSON --> Ext[External APIs]
```
**Components**
***RentVest.Web (Frontend)***:

    ASP.NET Core MVC application serving the UI.
    Acts as a REST Client using HttpClient.
    Handles Session storage for JWT tokens.
    RentVestGateway (Middleware):
    Ocelot API Gateway acting as a Reverse Proxy.
    Handles Routing, Rate Limiting (DDoS protection), and SSL Termination.`

    
***RentVest.Api (Backend)***:

    Core business logic and calculations.
    Manages database connections via Entity Framework Core.
    Orchestrates external API calls with caching strategies.

    
**🚀 Key Features**


🔐 Secure Authentication & RBAC:

    JWT (JSON Web Token) based authentication.
    Role-Based Access Control: Separate logic for User (Simulation) and Admin (System View).
    Secure password hashing using BCrypt.
💹 Intelligent Simulation Engine:

    Simulates portfolios based on Risk Tolerance (Conservative, Moderate, Aggressive).
    Calculates historical performance using real market data.
    Handles currency conversion (EUR/USD/GBP) based on historical exchange rates.
🌍 External Data Integration:

    Alpha Vantage: Fetches historical Stock/ETF prices (Cached in DB).
    Frankfurter: Fetches historical Currency Exchange rates.
    NewsAPI.ai: Fetches contextual financial news for the simulation period.
⚡ Performance & Resilience:

    Smart Caching: Local database caching minimizes external API calls and latency.
    Rate Limiting: Ocelot Gateway restricts request frequency to prevent abuse.
    Graceful Fallbacks: Handles external API failures or rate limits without crashing the user experience.


**🛠️ Tech Stack**


    Component	Technology
    Framework	.NET 8 (C#)
    Frontend	ASP.NET Core MVC, Bootstrap 5, JavaScript
    Gateway	Ocelot
    Database	SQLite (Dev) / SQL Server (Prod)
    ORM	Entity Framework Core
    Documentation	Swagger / OpenAPI


**⚙️ Setup & Configuration**

    Prerequisites
    .NET 8 SDK
    Visual Studio 2022 or VS Code
    API Keys (Required):
    Alpha Vantage (Stock Data)
    NewsAPI.ai (News Data)
    1. Clone the Repository
    code
    Bash
    git clone https://github.com/your-username/RentVest.git
    cd RentVest
    2. Configure Secrets (appsettings.json)
    You must configure the secrets in two locations. Ensure the JWT Secret Token matches in both files.
    Location 1: RentVest.Api/appsettings.json
    code
    JSON
    {
      "ConnectionStrings": {
        "DefaultConnection": "Data Source=rentvest.db"
      },
      "AppSettings": {
        "Token": "YOUR_SUPER_SECURE_LONG_SECRET_KEY_MUST_MATCH_GATEWAY"
      },
      "AlphaVantage": {
        "ApiKey": "YOUR_ALPHA_VANTAGE_KEY"
      },
      "News": {
        "ApiKey": "YOUR_NEWSAPI_KEY",
        "Url": "https://eventregistry.org/api/v1/article/getArticles"
      }
    }
    Location 2: RentVestGateway/appsettings.json
    code
    JSON
    {
      "AppSettings": {
        "Token": "YOUR_SUPER_SECURE_LONG_SECRET_KEY_MUST_MATCH_GATEWAY"
      }
    }
    3. Database Migration
    Initialize the database using Entity Framework Core.
    code
    Powershell
# Run in Package Manager Console or Terminal*
dotnet ef database update --project RentVest.Data --startup-project RentVest.Api
**▶️ How to Run**
To run the full system, all three projects must be running simultaneously.
Option A: Visual Studio (Recommended)
Right-click the Solution in Solution Explorer -> Properties.
Select Multiple Startup Projects.
Set the Action to Start for:
RentVest.Api
RentVestGateway
RentVest.Web
Press F5.
Option B: Terminal / CLI
Open 3 separate terminal windows:
Terminal 1 (Backend API):
code
Bash
cd RentVest.Api
dotnet run --urls="http://localhost:5095"
Terminal 2 (Gateway):
code
Bash
cd RentVestGateway
dotnet run --urls="http://localhost:5058"
Terminal 3 (Frontend Client):
code
Bash
cd RentVest.Web
dotnet run
🛡️ Usage Scenarios
1. User Registration & Login
Navigate to the web application.
Register a new account. The system hashes your password and stores it securely.
Log in to receive a JWT Token (handled automatically via Session).
2. Investment Simulation
Go to the Dashboard.
Input: Select a date in the past (e.g., 2 years ago), your monthly rent amount, and risk tolerance.
Process:
The system converts your currency to USD.
It checks the database for historical prices. If missing, it fetches them from Alpha Vantage.
It calculates how much profit/loss you would have made.
It fetches relevant news headlines from that specific time period.
Output: A detailed report of Portfolio Allocation, Total Profit, and Contextual News.
3. Admin Panel
Note: To become an admin, manually update your user Role to 'Admin' in the database.
Log in as an Admin.
Access the Admin Panel link in the navbar to view all registered users and their risk profiles.
