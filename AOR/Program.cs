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

// Kjør Docker Compose synkront i Development og vent på tjenester før appen starter
if (builder.Environment.IsDevelopment())
{
    // Juster tidsfrister etter behov
    var composeTimeout = TimeSpan.FromMinutes(5);

    EnsureComposeUpBlocking(
        composeFilePath: Path.Combine(builder.Environment.ContentRootPath, "docker-compose.yml"),
        servicesToWaitFor: new[]
        {
            // Vent på MariaDB-port lokalt
            ("mariadb", "localhost", 3306),
            // Vent på at web-containeren eksponerer /db-health via port 5001 på host (docker-compose.yml: "5001:8080")
            ("aor-web", "localhost", 5001)
        },
        healthChecks: new[]
        {
            // Ekstra: ping webens health-endepunkt
            ("aor-web", new Uri("http://localhost:5001/db-health"))
        },
        timeout: composeTimeout
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

// Legg til cookie-autentisering for å holde påloggingsstatus i en sikker cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AOR.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax; // nyttig lokalt
        options.LoginPath = "/LogIn";
        options.AccessDeniedPath = "/LogIn/AccessDenied";
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

app.UseStaticFiles(); // sikrer at statiske filer serveres når MapStaticAssets ikke finnes

app.UseRouting();

// Viktig: autentisering må komme før autorisasjon og før endepunkter
app.UseAuthentication();
app.UseAuthorization();

// MapStaticAssets() kan brukes hvis tilgjengelig, men UseStaticFiles dekker vanlig scenario
// app.MapStaticAssets();

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
    pattern: "{controller=LogIn}/{action=Index}/{id?}");

app.Run();

// Hjelpemetoder
static void EnsureComposeUpBlocking(
    string composeFilePath,
    (string service, string host, int port)[] servicesToWaitFor,
    (string service, Uri url)[] healthChecks,
    TimeSpan timeout)
{
    if (!File.Exists(composeFilePath))
        throw new FileNotFoundException("docker-compose.yml ikke funnet", composeFilePath);

    Console.WriteLine($"[compose] Starter: docker compose -f \"{composeFilePath}\" up -d");
    var upOk = RunProcessBlocking(
        fileName: "docker",
        arguments: $"compose -f \"{composeFilePath}\" up -d",
        workingDirectory: Path.GetDirectoryName(composeFilePath) ?? Directory.GetCurrentDirectory(),
        timeout: timeout,
        logPrefix: "[compose]"
    );
    if (!upOk)
        throw new InvalidOperationException("docker compose up feilet eller timet ut.");

    var start = DateTime.UtcNow;
    foreach (var (service, host, port) in servicesToWaitFor)
    {
        Console.WriteLine($"[wait] Venter på {service} port {host}:{port} (timeout {timeout.TotalSeconds}s)...");
        while (DateTime.UtcNow - start < timeout)
        {
            if (IsPortOpen(host, port, TimeSpan.FromSeconds(1)))
            {
                Console.WriteLine($"[wait] {service} port {port} er oppe.");
                break;
            }
            Thread.Sleep(1000);
        }
        if (!IsPortOpen(host, port, TimeSpan.FromSeconds(1)))
            throw new TimeoutException($"Timeout ved venting på {service} port {port}.");
    }

    using var http = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
    foreach (var (service, url) in healthChecks)
    {
        Console.WriteLine($"[wait] Venter på health {service} {url} (timeout {timeout.TotalSeconds}s)...");
        var ok = false;
        while (DateTime.UtcNow - start < timeout)
        {
            try
            {
                var resp = http.GetAsync(url).GetAwaiter().GetResult();
                if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 500)
                {
                    ok = true;
                    Console.WriteLine($"[wait] {service} health OK ({(int)resp.StatusCode}).");
                    break;
                }
            }
            catch { /* ignorer til neste forsøk */ }
            Thread.Sleep(1000);
        }
        if (!ok)
            throw new TimeoutException($"Timeout ved venting på health for {service} ({url}).");
    }
}

static bool RunProcessBlocking(string fileName, string arguments, string workingDirectory, TimeSpan timeout, string logPrefix)
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
        },
        EnableRaisingEvents = true
    };

    var exited = new ManualResetEventSlim(false);
    p.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine($"{logPrefix} {e.Data}"); };
    p.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine($"{logPrefix} ERR: {e.Data}"); };
    p.Exited += (_, __) => exited.Set();

    Console.WriteLine($"{logPrefix} Kjører: {fileName} {arguments}");
    if (!p.Start())
        return false;

    p.BeginOutputReadLine();
    p.BeginErrorReadLine();

    var finishedInTime = exited.Wait(timeout);
    if (!finishedInTime)
    {
        try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch { }
        Console.WriteLine($"{logPrefix} Prosess timet ut.");
        return false;
    }

    Console.WriteLine($"{logPrefix} ExitCode={p.ExitCode}");
    return p.ExitCode == 0;
}

static bool IsPortOpen(string host, int port, TimeSpan timeout)
{
    try
    {
        using var cts = new CancellationTokenSource(timeout);
        using var client = new System.Net.Sockets.TcpClient();
        var task = client.ConnectAsync(host, port);
        task.Wait(cts.Token);
        return client.Connected;
    }
    catch { return false; }
}

