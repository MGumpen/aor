using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.AddMySqlDataSource(connectionName: "mysqldb");
var app = builder.Build();

app.MapDefaultEndpoints();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/dbcheck", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("DefaultConnection");
    try
    {
        using var conn = new MySqlConnection(cs);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT 1", conn);
        var result = await cmd.ExecuteScalarAsync();
        return Results.Ok($"DB OK (SELECT 1 = {result})");
    }
    catch (Exception ex)
    {
        return Results.Problem("DB error: " + ex.Message);
    }
});


app.Run();
// testing