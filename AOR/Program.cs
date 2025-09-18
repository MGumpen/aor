using Microsoft.EntityFrameworkCore;
using AOR.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

/*
 CHANGE LOG — erfan
 Hva:
 1) Aktivert cookie-basert autentisering (AddAuthentication + AddCookie).
 2) Satt LoginPath = /LogIn og AccessDeniedPath = /LogIn/AccessDenied.
 3) Aktivert UseAuthentication() i middleware-pipelinen før UseAuthorization().
 Hvorfor:
 - For å huske innloggingsstatus på tvers av forespørsler og håndheve rollebasert tilgang (Registerfører).
 - For å sende ikke-innloggede til innlogging, og brukere uten rett rolle til AccessDenied.
*/

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Les flagg for in-memory DB (lokal kjøring uten DB)
// og connection string dersom DB er tilgjengelig
var useInMemory = builder.Configuration.GetValue<bool>("USE_INMEMORY_DB");
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

// Konfigurer DbContext betinget: InMemory når USE_INMEMORY_DB=true,
// ellers MariaDB (MySQL-kompatibel)
if (useInMemory || string.IsNullOrWhiteSpace(connStr))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("AOR_InMemory"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            connStr,
            ServerVersion.AutoDetect(connStr)
        ));
}


// Add Entity Framework with MariaDB

// Legg til cookie-autentisering for å holde påloggingsstatus i en sikker cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LogIn";                // Send brukere hit hvis de ikke er innlogget — erfan
        options.AccessDeniedPath = "/LogIn/AccessDenied"; // Send brukere hit hvis de mangler riktig rolle — erfan
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Engangs-sjekk ved oppstart: bare forsøk å sjekke DB hvis vi ikke kjører InMemory
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (!useInMemory && !string.IsNullOrWhiteSpace(connStr))
        {
            if (db.Database.CanConnect())
                logger.LogInformation("MariaDB-tilkobling OK.");
            else
                logger.LogWarning("MariaDB-tilkobling feilet (CanConnect() = false).");
        }
        else
        {
            logger.LogInformation("Kjører med InMemoryDatabase (ingen ekstern DB-tilkobling).");
        }
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

// Tillat å skru av HTTPS-redirect ved behov (f.eks. Docker eller lokal http-profil)
var disableHttpsRedirect = app.Configuration.GetValue<bool>("DISABLE_HTTPS_REDIRECT");
if (!disableHttpsRedirect)
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Health-endepunkt
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




// Viktig: Aktiver autentisering før autorisasjon — erfan
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

//Fører til LogIn siden når appen startes

app.Run();

