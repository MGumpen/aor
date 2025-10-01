using Microsoft.EntityFrameworkCore;
using AOR.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// DB-oppsett: ENV (docker) først, så appsettings.*
var cs =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrWhiteSpace(cs))
{
    builder.Services.AddDbContext<ApplicationDbContext>(o =>
        o.UseInMemoryDatabase("AOR_InMemory"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(o =>
        o.UseMySql(cs!, ServerVersion.AutoDetect(cs)));
}

// AuthenificationS
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LogIn";
        options.AccessDeniedPath = "/LogIn/AccessDenied";
    }
        );

var app = builder.Build();

// Configure pipeline
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
    pattern: "{controller=LogIn}/{action=Index}/{id?}");

app.Run();
