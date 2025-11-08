using courses_platform;
using courses_platform.Contexts;
using courses_platform.Models;
using courses_platform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;


var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration.GetValue<string>("DatabaseProvider")?.ToLower();

var sqlServerConn = builder.Configuration.GetConnectionString("SqlServerConnection");
var postgreConn = builder.Configuration.GetConnectionString("PostgreSQLConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SQLiteConnection");

switch (provider)
{
    case "postgres":
    case "postgresql":
        builder.Services.AddDbContext<ApplicationDbContext, ApplicationDbContextPostgreSQL>(options =>
            options.UseNpgsql(postgreConn));
        break;

    case "sqlite":
        builder.Services.AddDbContext<ApplicationDbContext, ApplicationDbContextSqlite>(options =>
            options.UseSqlite(sqliteConn));
        break;

    case "inmemory":
        builder.Services.AddDbContext<ApplicationDbContext, ApplicationDbContextInMemory>(options =>
            options.UseInMemoryDatabase("CoursesPlatform_InMemory"));
        break;

    case "sqlserver":
    default:
        builder.Services.AddDbContext<ApplicationDbContext, ApplicationDbContextSqlServer>(options =>
            options.UseSqlServer(sqlServerConn));
        break;
}

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<CloudinaryService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<UserApiService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = "https://localhost:5000";
    options.ClientId = "courses_platform_client";
    options.ClientSecret = "secret";
    options.ResponseType = "code";

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("api1");
    options.Scope.Add("roles");

    options.ClaimActions.MapJsonKey("sub", "sub");
    options.ClaimActions.MapJsonKey("role", "role");
    options.ClaimActions.MapJsonKey("email", "email");
    options.ClaimActions.MapJsonKey("name", "name");

    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.SignedOutRedirectUri = "https://localhost:5001/";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var sub = ctx.Principal.FindFirst("sub")?.Value ??
                        ctx.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(sub))
            {
                bool exists = await db.AppUsers.AnyAsync(u => u.ExternalUserId == sub);
                if (!exists)
                {
                    db.AppUsers.Add(new AppUser { ExternalUserId = sub });
                    await db.SaveChangesAsync();
                }
            }
        },

        OnTicketReceived = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var sub = ctx.Principal.FindFirst("sub")?.Value ??
                        ctx.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(sub))
                return;

            var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.ExternalUserId == sub);
            if (appUser == null)
            {
                appUser = new AppUser { ExternalUserId = sub };
                db.AppUsers.Add(appUser);
                await db.SaveChangesAsync();
            }

            var roleClaim = ctx.Principal.FindFirst("role")?.Value ??
                            ctx.Principal.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (!string.IsNullOrEmpty(roleClaim))
            {
                // підставляємо id користувача в URL
                switch (roleClaim)
                {
                    case "Student":
                        ctx.ReturnUri = $"/Account/ProfileStudent/{appUser.Id}";
                        break;
                    case "Professor":
                        ctx.ReturnUri = $"/Account/ProfileProfessor/{appUser.Id}";
                        break;
                    case "Admin":
                        ctx.ReturnUri = "/Admin/Index";
                        break;
                }
            }
        },

        OnRedirectToIdentityProvider = context =>
        {
            if (context.Properties.Parameters.TryGetValue("role", out var role))
            {
                context.ProtocolMessage.SetParameter("expectedRole", role?.ToString());
            }

            return Task.CompletedTask;
        }
    };


});

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo("./keys"))
//    .SetApplicationName("CoursesPlatformClient");

//builder.WebHost.ConfigureKestrel(options =>
//{
//    var basePath = builder.Environment.ContentRootPath;
//    var certPath = Path.Combine(basePath, "certs", "localhost+2.pem");
//    var keyPath = Path.Combine(basePath, "certs", "localhost+2-key.pem");

//    if (File.Exists(certPath) && File.Exists(keyPath))
//    {
//        options.ListenLocalhost(5001, listenOptions =>
//        {
//            listenOptions.UseHttps(certPath, keyPath);
//        });
//        Console.WriteLine($"[Kestrel] HTTPS: using mkcert ({certPath})");
//    }
//    else
//    {
//        throw new FileNotFoundException($"Certificate not found: {certPath}");
//    }
//});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData.Initialize(db);
}

app.Run();

