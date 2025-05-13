using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pojisteni.Models;
using Pojisteni.Services;

namespace Pojisteni.Controllers
{
    /// <summary>
    /// Kontroler pro správu záznamů pojištěných osob a jejich účtů.
    /// </summary>
    public class InsuredPersonController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Konstruktor přijímá závislosti pro práci s databází a správu Identity uživatelů.
        /// </summary>
        /// <param name="context">Databázový kontext aplikace.</param>
        /// <param name="userManager">Správce uživatelů Identity.</param>
        public InsuredPersonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        /// <summary>
        /// Zobrazí seznam všech pojištěných osob, případně filtrovaný podle jména nebo příjmení.
        /// </summary>
        /// <param name="search">Volitelný řetězec pro vyhledávání v křestních a příjmeních.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Search"] = search;
            var insuredPersonList = search != null
                ? await context.InsuredPersons
                    .Include(x => x.ApplicationUser)
                    .Where(x => x.FirstName.Contains(search) || x.LastName.Contains(search))
                    .OrderBy(x => x.FirstName)
                    .ToListAsync()
                : await context.InsuredPersons
                    .Include(x => x.ApplicationUser)
                    .OrderBy(x => x.FirstName)
                    .ToListAsync();

            return View(insuredPersonList);
        }

        /// <summary>
        /// Zobrazí formulář pro vytvoření nové pojištěné osoby.
        /// </summary>
        [Authorize(Roles = Role.admin)]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Zpracuje odeslaný formulář pro vytvoření pojištěné osoby a odpovídajícího uživatelského účtu.
        /// </summary>
        /// <param name="insuredViewModel">ViewModel obsahující údaje nové osoby a heslo.</param>
        [HttpPost]
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Create(InsuredPersonDetailViewModel insuredViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(insuredViewModel);
            }

            var user = new ApplicationUser
            {
                UserName = insuredViewModel.Email,
                Email = insuredViewModel.Email,
                Firstname = insuredViewModel.FirstName,
                Lastname = insuredViewModel.LastName,
                Address = insuredViewModel.Address,
            };

            var userWithEmail = await userManager.FindByEmailAsync(user.Email);
            if (userWithEmail != null)
            {
                ModelState.AddModelError(nameof(insuredViewModel.Email), "Tento e-mail má již jiný uživatel");
                insuredViewModel.Email = "";
                return View(insuredViewModel);
            }

            var result = await userManager.CreateAsync(user, insuredViewModel.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(insuredViewModel);
            }

            var insuredPerson = new InsuredPerson
            {
                FirstName = insuredViewModel.FirstName,
                LastName = insuredViewModel.LastName,
                Address = insuredViewModel.Address,
                DateOfBirth = insuredViewModel.DateOfBirth,
                ApplicationUserId = user.Id
            };

            context.InsuredPersons.Add(insuredPerson);
            await context.SaveChangesAsync();
            await userManager.AddToRoleAsync(user, Role.client);
            TempData["Message"] = $"Pojištěnec: {insuredPerson.FirstName} {insuredPerson.LastName} byl vytvořen";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Odstraní pojištěnou osobu a související uživatelský účet podle e-mailu.
        /// </summary>
        /// <param name="email">E-mail uživatele, jehož záznamy se mají odstranit.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Delete(string email)
        {
            var appUser = await userManager.FindByEmailAsync(email);
            if (appUser != null)
            {
                var client = await context.InsuredPersons
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == appUser.Id);

                if (client != null)
                {
                    context.InsuredPersons.Remove(client);
                    await context.SaveChangesAsync();

                    var result = await userManager.DeleteAsync(appUser);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = $"Pojištěnec: {client.FirstName} {client.LastName} byl odstraněn";
                    }
                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Zobrazí formulář pro úpravu údajů pojištěné osoby na základě e-mailu.
        /// </summary>
        /// <param name="email">E-mail uživatele, jehož údaje se mají upravit.</param>
        [Authorize(Roles = Role.admin + "," + Role.client)]
        public async Task<IActionResult> Edit(string email)
        {
            var appUser = await userManager.FindByEmailAsync(email);
            if (appUser == null)
            {
                return RedirectToAction("Index");
            }

            var person = await context.InsuredPersons
                .FirstOrDefaultAsync(x => x.ApplicationUserId == appUser.Id);
            if (person == null)
            {
                return RedirectToAction("Index");
            }

            var model = new InsuredPersonDetailViewModel
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Address = person.Address,
                DateOfBirth = person.DateOfBirth,
                Email = appUser.Email!
            };
            return View(model);
        }

        /// <summary>
        /// Zpracuje odeslanou úpravu údajů pojištěné osoby a aktualizuje databázi.
        /// </summary>
        /// <param name="model">ViewModel s aktualizovanými údaji pojištěné osoby.</param>
        [HttpPost]
        [Authorize(Roles = Role.admin + "," + Role.client)]
        public async Task<IActionResult> Edit(InsuredPersonDetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appUser = await userManager.FindByEmailAsync(model.Email);
            if (appUser == null)
            {
                return RedirectToAction("Index");
            }

            var insuredPerson = await context.InsuredPersons
                .FirstOrDefaultAsync(x => x.ApplicationUserId == appUser.Id);
            if (insuredPerson == null)
            {
                return RedirectToAction("Index");
            }

            insuredPerson.FirstName = model.FirstName;
            insuredPerson.LastName = model.LastName;
            insuredPerson.Address = model.Address;
            insuredPerson.DateOfBirth = model.DateOfBirth;

            appUser.Email = model.Email;
            appUser.UserName = model.Email;

            var result = await userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                return View(model);
            }

            await context.SaveChangesAsync();
            TempData["Message"] = $"Pojištěnec: {insuredPerson.FirstName} {insuredPerson.LastName} byl upraven";

            if (User.IsInRole(Role.admin))
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Detail", new { email = model.Email });
        }

        /// <summary>
        /// Zobrazí detail pojištěné osoby včetně jejích smluv a událostí.
        /// </summary>
        /// <param name="email">E-mail pojištěné osoby, jejíž detail se zobrazí.</param>
        [Authorize(Roles = Role.admin + "," + Role.client)]
        public async Task<IActionResult> Detail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var insuredPerson = await context.InsuredPersons
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);
            if (insuredPerson == null)
            {
                return RedirectToAction("Index");
            }

            var agreedInsurances = await context.AgreedInsurances
                .Include(x => x.Insurance)
                .Where(x => x.InsuredPersonId == insuredPerson.Id)
                .ToListAsync();

            var insuranceEvents = await context.InsuranceEvents
                .Where(x => x.InsuredPersonId == insuredPerson.Id)
                .OrderByDescending(x => x.OccurredOn)
                .ToListAsync();

            var viewModel = new InsuredPersonAndInsuraceDetailViewModel
            {
                Id = insuredPerson.Id,
                FirstName = insuredPerson.FirstName,
                LastName = insuredPerson.LastName,
                Address = insuredPerson.Address,
                DateOfBirth = insuredPerson.DateOfBirth,
                Email = user.Email!,
                AgreedInsurances = agreedInsurances,
                InsuranceEvents = insuranceEvents
            };

            return View(viewModel);
        }
    }
}
