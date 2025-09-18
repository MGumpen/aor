using Microsoft.EntityFrameworkCore;
using AOR.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework with MariaDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

var app = builder.Build();

// Engangs-sjekk ved oppstart (logger resultatet)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Synkron for enkelhet; kan også bruke CanConnectAsync().GetAwaiter().GetResult()
        if (db.Database.CanConnect())
            logger.LogInformation("MariaDB-tilkobling OK.");
        else
            logger.LogWarning("MariaDB-tilkobling feilet (CanConnect() = false).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Feil ved tilkobling til MariaDB under oppstart.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Generelt health-endepunkt
app.MapGet("/health", () => Results.Ok("OK"));

// Lettvekts health-endepunkt for DB: returnerer 200 når DB er tilgjengelig
app.MapGet("/db-health", async (ApplicationDbContext db) =>
{
    try
    {
        var can = await db.Database.CanConnectAsync();
        return can
            ? Results.Ok("OK: Tilkoblet MariaDB")
            : Results.StatusCode(503);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 503,
            title: "Kunne ikke koble til MariaDB");
    }
});

//Fører til LogIn siden når appen startes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LogIn}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
