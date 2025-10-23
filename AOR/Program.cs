using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using AOR.Data;
using AOR.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Cookie auth
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/LogIn/Index";
        o.LogoutPath = "/LogIn/Logout";
        o.AccessDeniedPath = "/LogIn/AccessDenied";
        o.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
// CLEAN database configuration - no orchestration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AOR_InMemory"));


// DB
var cs = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

var app = builder.Build();

// Prod-feilside + HSTS
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

// ---------- AUTO-MIGRATE + (OPTIONAL) SEED ----------
if (app.Configuration.GetValue<bool>("RunMigrationsOnStart", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (app.Configuration.GetValue<bool>("SeedDemoData", false))
    {
        if (!db.ObstacleDatas.Any())
        {
            db.ObstacleDatas.Add(new ObstacleData
            {
                ObstacleName = "Demo Tower",
                ObstacleType = "tower",
                Coordinates  = "[[58.16, 8.00]]",
                PointCount   = 1,
                CreatedAt    = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}
// ----------------------------------------------------

// Debug-endepunkter (nyttige for sensur/test)
static string Mask(string s) =>
    Regex.Replace(s ?? "(empty)", @"(?i)(password|pwd)=([^;]+)", "$1=****");

app.MapGet("/db-health", async (ApplicationDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok("DB OK") : Results.Problem("DB DOWN"));

app.MapGet("/debug/db-info", async (ApplicationDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return Results.Ok(new
    {
        Connected = ok,
        Provider = db.Database.ProviderName,
        ConnectionString = Mask(db.Database.GetDbConnection().ConnectionString)
    });
});

app.MapGet("/debug/obstacles", async (ApplicationDbContext db) =>
{
    var count  = await db.ObstacleDatas.CountAsync();
    var latest = await db.ObstacleDatas
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new { x.Id, x.ObstacleName, x.CreatedAt })
        .Take(5)
        .ToListAsync();

    return Results.Ok(new { Count = count, Latest = latest });
});

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LogIn}/{action=Index}/{id?}");

app.Run();