using Microsoft.EntityFrameworkCore;
using AOR.Data;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Database configuration - MySQL
var connectionString = builder.Configuration.GetConnectionString("AorDb")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));
    builder.Services.AddDbContext<AorDbContext>(options =>
        options.UseMySql(connectionString, serverVersion));
}
else
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddDbContext<AorDbContext>(options =>
            options.UseInMemoryDatabase("AorDevInMemory"));
    }
}


// Identity with roles and cookie authentication
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        // Password settings (optional - configure as needed)
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<AorDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie authentication paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/LogIn";
    options.AccessDeniedPath = "/LogIn/AccessDenied";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AorDbContext>();
        var provider = db.Database.ProviderName ?? string.Empty;
        if (!provider.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            db.Database.Migrate();
        }
        else
        {
            logger.LogInformation("InMemory provider detected; skipping migrations.");
        }
    }
    catch (DbUpdateException ex)
    {
        logger.LogError(ex, "Database migration failed due to a database update error; continuing without applying migrations.");
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "Database migration failed due to an invalid operation; continuing without applying migrations.");
    }

    // Seed test users and roles
    try
    {
        await AorDbSeeder.SeedAsync(scope.ServiceProvider, logger);
    }
    catch (DbUpdateException ex)
    {
        logger.LogError(ex, "Seeding failed due to a database update error");
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "Seeding failed due to an invalid operation");
    }
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
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

//Kjør