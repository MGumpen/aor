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

// Add Entity Framework with MariaDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Legg til cookie-autentisering for å holde påloggingsstatus i en sikker cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LogIn";                // Send brukere hit hvis de ikke er innlogget — erfan
        options.AccessDeniedPath = "/LogIn/AccessDenied"; // Send brukere hit hvis de mangler riktig rolle — erfan
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Viktig: Aktiver autentisering før autorisasjon — erfan
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

//Fører til LogIn siden når appen startes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LogIn}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
