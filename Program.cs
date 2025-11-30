using courses_platform;
using courses_platform.Contexts;
using courses_platform.Models;
using courses_platform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration.GetValue<string>("DatabaseProvider")?.ToLower();

Console.WriteLine($"Using database provider: {provider}");

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
    options.Authority = "https://localhost:5005";
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

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(2, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true; 
    opt.ApiVersionReader = new UrlSegmentApiVersionReader(); // /api/v{version}/[controller] instead of ?api-version=version
});

builder.Services.AddVersionedApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'VVV"; // v1, v2, etc.
    opt.SubstituteApiVersionInUrl = true;
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var desc in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

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

