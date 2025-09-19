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

// Kjør Docker Compose automatisk i Development
if (builder.Environment.IsDevelopment())
{
    await EnsureComposeUpAsync(
        composeFilePath: Path.Combine(builder.Environment.ContentRootPath, "docker-compose.yml"),
        servicesToWaitFor: new[] { ("aor-db", 3306) },
        timeout: TimeSpan.FromMinutes(2),
        cancellationToken: CancellationToken.None
    );
}

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

// Viktig: autentisering må komme før autorisasjon og før endepunkter
app.UseAuthentication();
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

app.Run();

// Hjelpemetoder
static async Task EnsureComposeUpAsync(string composeFilePath, (string service, int port)[] servicesToWaitFor, TimeSpan timeout, CancellationToken cancellationToken)
{
    if (!File.Exists(composeFilePath))
        return; // valgfritt: throw om compose er påkrevd

    // docker compose up -d
    var upOk = await RunProcessAsync(
        fileName: "docker",
        arguments: $"compose -f \"{composeFilePath}\" up -d",
        workingDirectory: Path.GetDirectoryName(composeFilePath) ?? Directory.GetCurrentDirectory(),
        timeout: timeout,
        cancellationToken: cancellationToken
    );

    if (!upOk)
        throw new InvalidOperationException("Klarte ikke å kjøre 'docker compose up -d'.");

    // Vent på tjenester (enkel port-sjekk; bruk healthcheck via CLI om ønskelig)
    var start = DateTime.UtcNow;
    foreach (var (_, port) in servicesToWaitFor)
    {
        while (DateTime.UtcNow - start < timeout)
        {
            if (await IsPortOpenAsync("localhost", port, TimeSpan.FromSeconds(1)))
                break;
            await Task.Delay(1000, cancellationToken);
        }
    }
}

static async Task<bool> RunProcessAsync(string fileName, string arguments, string workingDirectory, TimeSpan timeout, CancellationToken cancellationToken)
{
    using var p = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };
    var tcs = new TaskCompletionSource<bool>();
    p.EnableRaisingEvents = true;
    p.Exited += (_, __) => tcs.TrySetResult(p.ExitCode == 0);

    p.Start();

    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(timeout);

    var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
    if (completed != tcs.Task)
    {
        try { if (!p.HasExited) p.Kill(true); } catch { }
        return false;
    }
    return await tcs.Task;
}

static async Task<bool> IsPortOpenAsync(string host, int port, TimeSpan timeout)
{
    try
    {
        using var cts = new CancellationTokenSource(timeout);
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(host, port, cts.Token);
        return client.Connected;
    }
    catch { return false; }
}

