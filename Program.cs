using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pojisteni.Services;
using Pojisteni.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Vytvo�� konfiguraci a hostitele webov� aplikace na z�klad� vstupn�ch argument�.

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
// P�id� podporu MVC (kontrolery + view) a automaticky validuje antiforgery token u v�ech POST request�.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});
// Konfiguruje Entity Framework Core pro SQL Server s p�ipojovac�m �et�zcem "DefaultConnection" z appsettings.json.

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();
// P�id� ASP.NET Core Identity pro spr�vu u�ivatel� a rol�,
// nastav� minim�ln� d�lku hesla a dal�� pravidla a ulo�� data pomoc� EF Core.

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
// Definuje cesty pro p�ihl�en� a str�nku p�i odep�en�m p��stupu.

builder.WebHost.UseUrls("http://*:5000");
// Aplikuje v�choz� URL, na kter�ch bude aplikace naslouchat.

var app = builder.Build();
// Sestav� fin�ln� pipeline webov� aplikace na z�klad� v��e uveden�ch konfigurac�.

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
// V produk�n�m prost�ed� pou��v� HSTS pro zv��en� bezpe�nosti (nut� prohl�e� komunikovat pouze po HTTPS).

app.UseHttpsRedirection();
// P�esm�ruje v�echny HTTP po�adavky na HTTPS.

app.UseRouting();
// Aktivuje routovac� middleware, kter� sm�ruje po�adavky na p��slu�n� kontrolery.

app.UseAuthorization();
// Zapne autorizaci � ov��uje, zda m� p�ihl�en� u�ivatel p��stup k dan�m akc�m.

app.MapStaticAssets();
// Mapuje a zp��stup�uje statick� soubory (CSS, JS, obr�zky apod.) ze slo�ky wwwroot.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
// Nastav� v�choz� routu: pokud nen� zad�n jin� kontroler �i akce, pou�ije se Account/Login.

using (var scope = app.Services.CreateAsyncScope())
{
    var userManager = scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>))
        as UserManager<ApplicationUser>;
    var roleManager = scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>))
        as RoleManager<IdentityRole>;

    // Napln� datab�zi v�choz�mi rolemi a u�ivateli p�i spu�t�n� aplikace.
    await DatabaseInitializer.InitializeDataAsync(userManager, roleManager);
}

app.Run();
// Spust� webov� server a za�ne naslouchat na definovan�ch URL. 