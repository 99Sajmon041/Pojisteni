using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pojisteni.Services;
using Pojisteni.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Vytvoøí konfiguraci a hostitele webové aplikace na základì vstupních argumentù.

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
// Pøidá podporu MVC (kontrolery + view) a automaticky validuje antiforgery token u všech POST requestù.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});
// Konfiguruje Entity Framework Core pro SQL Server s pøipojovacím øetìzcem "DefaultConnection" z appsettings.json.

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();
// Pøidá ASP.NET Core Identity pro správu uživatelù a rolí,
// nastaví minimální délku hesla a další pravidla a uloží data pomocí EF Core.

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
// Definuje cesty pro pøihlášení a stránku pøi odepøeném pøístupu.

builder.WebHost.UseUrls("http://*:5000");
// Aplikuje výchozí URL, na kterých bude aplikace naslouchat.

var app = builder.Build();
// Sestaví finální pipeline webové aplikace na základì výše uvedených konfigurací.

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
// V produkèním prostøedí používá HSTS pro zvýšení bezpeènosti (nutí prohlížeè komunikovat pouze po HTTPS).

app.UseHttpsRedirection();
// Pøesmìruje všechny HTTP požadavky na HTTPS.

app.UseRouting();
// Aktivuje routovací middleware, který smìruje požadavky na pøíslušné kontrolery.

app.UseAuthorization();
// Zapne autorizaci – ovìøuje, zda má pøihlášený uživatel pøístup k daným akcím.

app.MapStaticAssets();
// Mapuje a zpøístupòuje statické soubory (CSS, JS, obrázky apod.) ze složky wwwroot.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
// Nastaví výchozí routu: pokud není zadán jiný kontroler èi akce, použije se Account/Login.

using (var scope = app.Services.CreateAsyncScope())
{
    var userManager = scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>))
        as UserManager<ApplicationUser>;
    var roleManager = scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>))
        as RoleManager<IdentityRole>;

    // Naplní databázi výchozími rolemi a uživateli pøi spuštìní aplikace.
    await DatabaseInitializer.InitializeDataAsync(userManager, roleManager);
}

app.Run();
// Spustí webový server a zaène naslouchat na definovaných URL. 