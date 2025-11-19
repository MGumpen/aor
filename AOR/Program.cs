using Microsoft.EntityFrameworkCore;
using AOR.Data;
using AOR.Repositories;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Database configuration - MySQL
var connectionString = builder.Configuration.GetConnectionString("AorDb");
builder.Services.AddDbContext<AorDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(11, 4, 0))));

builder.Services.AddScoped<IObstacleRepository, ObstacleRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Identity - registered after DbContext so stores are available
// Use full AddIdentity so SignInManager, UserManager, RoleManager and cookie handling are configured
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Optional: tweak password requirements for dev/test convenience
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<AorDbContext>()
    .AddDefaultTokenProviders();

// Authorization policies basert på ActiveRole-claim
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AsAdmin",     p => p.RequireClaim("ActiveRole", "Admin"));
    options.AddPolicy("AsCrew",      p => p.RequireClaim("ActiveRole", "Crew"));
    options.AddPolicy("AsRegistrar", p => p.RequireClaim("ActiveRole", "Registrar"));
});

// Configure application cookie (custom login path)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/LogIn";
    options.AccessDeniedPath = "/LogIn/AccessDenied";
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AorDbContext>();
    db.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    try
    {
        var db = sp.GetRequiredService<AOR.Data.AorDbContext>();

        // Migrate + seed inside try so app doesn't crash when DB isn't reachable
        await db.Database.MigrateAsync();                 // <- migrate

        // Hent logger fra DI og pass både service provider og logger til seederen
        var logger = sp.GetRequiredService<ILogger<Program>>();
        await AOR.Data.AorDbSeeder.SeedAsync(sp, logger);  // <- SEED (med riktige argumenter)
    }
    catch (Exception ex)
    {
        // If DB isn't reachable or migration fails, log and continue so web app can start
        var logger = sp.GetService<ILogger<Program>>();
        if (logger != null)
        {
            logger.LogError(ex, "Database migrate/seed failed at startup. Application will continue to run.");
        }
        else
        {
            Console.Error.WriteLine("Database migrate/seed failed: " + ex);
        }
    }
}


// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Disable caching so user cannot go back after logout
app.Use(async (context, next) =>
{   
    context.Response.Headers.Append("Cache-Control", "xss-protection=1; mode=block");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' https://cdnjs.cloudflare.com https://unpkg.com https://kit.fontawesome.com https://ajax.googleapis.com 'unsafe-inline';  " + // Unsafe-inline only for dev. Remove for production. Use either hash or nonce.
        "style-src 'self' https://cdnjs.cloudflare.com https://unpkg.com https://fonts.googleapis.com 'unsafe-inline'; " + // Unsafe-inline only for dev. Remove for production. Use either hash or nonce.
        "font-src 'self' https://fonts.gstatic.com https://unpkg.com https://ka-f.fontawesome.com https://kit.fontawesome.com https://cdnjs.cloudflare.com data:; " + 
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://api.kartverket.no https://ka-f.fontawesome.com https://nominatim.openstreetmap.org/; " + 
        "object-src 'none';");
    
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LogIn}/{action=Index}/{id?}");

app.Run();
