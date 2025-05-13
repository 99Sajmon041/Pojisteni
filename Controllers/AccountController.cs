using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pojisteni.Models;
using Pojisteni.Services;
using System.Threading.Tasks;

namespace Pojisteni.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.context = context;
        }

        /// <summary>
        /// Zobrazí stránku pro přihlášení. Pokud je uživatel již přihlášen,
        /// přesměruje ho buď na seznam pojištěných (pro admina), 
        /// nebo na detail jeho pojištěné osoby (pro klienta).
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                if (User.IsInRole(Role.admin))
                {
                    return RedirectToAction("Index", "InsuredPerson");
                }
                else
                {
                    var appUser = await userManager.GetUserAsync(User);
                    var insuredPerson = await context.InsuredPersons.FirstOrDefaultAsync(x =>
                        x.ApplicationUserId == appUser!.Id);

                    if (insuredPerson != null)
                    {
                        string personEmail = insuredPerson.ApplicationUser!.Email!;
                        return RedirectToAction("Detail", "InsuredPerson", new { email = personEmail });
                    }
                    return View();
                }
            }
            return View();
        }

        /// <summary>
        /// Zpracuje POST přihlášení: ověří model, najde uživatele podle emailu,
        /// zkontroluje heslo a přihlásí ho. Po úspěšném přihlášení
        /// přesměruje admina na seznam pojištěných, klienta na detail jeho pojištěného.
        /// </summary>
        /// <param name="loginViewModel">Data z přihlášovacího formuláře (Email, Password, RememberMe).</param>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var appUser = await userManager.FindByEmailAsync(loginViewModel.Email);
            if (appUser == null)
            {
                ModelState.AddModelError(nameof(loginViewModel.Email), "Uživatel s tímto emailem neexistuje.");
                return View(loginViewModel);
            }

            var passwordValid = await userManager.CheckPasswordAsync(appUser, loginViewModel.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError(nameof(loginViewModel.Password), "Zadal jste špatné heslo!");
                return View(loginViewModel);
            }

            await signInManager.SignInAsync(appUser, loginViewModel.RememberMe);

            var roles = await userManager.GetRolesAsync(appUser);
            if (roles.Contains(Role.admin))
            {
                return RedirectToAction("Index", "InsuredPerson");
            }
            else
            {
                var insuredPerson = await context.InsuredPersons
                    .Include(x => x.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == appUser.Id);

                if (insuredPerson != null)
                {
                    string personEmail = insuredPerson.ApplicationUser!.Email!;
                    return RedirectToAction("Detail", "InsuredPerson", new { email = personEmail });
                }
            }
            return View(loginViewModel);
        }

        /// <summary>
        /// Odhlásí aktuálně přihlášeného uživatele a přesměruje na přihlašovací stránku.
        /// </summary>
        [Authorize(Roles = Role.admin + "," + Role.client)]
        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();
            }
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Zobrazí stránku s informací o odepřeném přístupu (403).
        /// </summary>
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
