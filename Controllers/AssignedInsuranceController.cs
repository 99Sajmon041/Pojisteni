using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pojisteni.Models;
using Pojisteni.Services;

namespace Pojisteni.Controllers
{
    /// <summary>
    /// Kontroler pro správu sjednaných pojištění (přiřazení pojištění osobám).
    /// </summary>
    public class AssignedInsuranceController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Konstruktor přijímá kontext databáze a správce uživatelů Identity.
        /// </summary>
        public AssignedInsuranceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        /// <summary>
        /// Zobrazí seznam všech sjednaných pojištění, případně filtrovaný podle jména nebo příjmení.
        /// </summary>
        /// <param name="search">Volitelný vyhledávací text pro jméno nebo příjmení pojištěné osoby.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Index(string? search)
        {
            List<AgreedInsurance> agreedInsurances;

            if (search != null)
            {
                agreedInsurances = await context.AgreedInsurances
                    .Include(x => x.Insurance)
                    .Include(x => x.InsuredPerson)
                    .Where(x => x.InsuredPerson!.FirstName.Contains(search)
                             || x.InsuredPerson.LastName.Contains(search))
                    .OrderBy(x => x.ValidTo)
                    .ToListAsync();
            }
            else
            {
                agreedInsurances = await context.AgreedInsurances
                    .Include(x => x.Insurance)
                    .Include(x => x.InsuredPerson)
                    .OrderBy(x => x.ValidTo)
                    .ToListAsync();
            }

            ViewData["Search"] = search;
            return View(agreedInsurances);
        }

        /// <summary>
        /// Odstraní sjednané pojištění podle jeho Id a přesměruje na detail pojištěné osoby.
        /// </summary>
        /// <param name="id">Identifikátor sjednaného pojištění k odstranění.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var agreedInsurance = await context.AgreedInsurances
                .Include(x => x.Insurance)
                .Include(x => x.InsuredPerson)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (agreedInsurance == null)
                return RedirectToAction("Index");

            context.AgreedInsurances.Remove(agreedInsurance);
            await context.SaveChangesAsync();

            TempData["Message"] = $"Sjednané pojištění: {agreedInsurance.Insurance!.Description} pro: {agreedInsurance.InsuredPerson!.FullName} odstraněno";

            var user = await context.InsuredPersons
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == agreedInsurance.InsuredPersonId);

            string personEmail = user!.ApplicationUser!.Email!;
            return RedirectToAction("Detail", "InsuredPerson", new { email = personEmail });
        }

        /// <summary>
        /// Zobrazí detail konkrétního sjednaného pojištění.
        /// </summary>
        /// <param name="id">Identifikátor sjednaného pojištění.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Detail(int id)
        {
            var agreedInsurance = await context.AgreedInsurances
                .Include(ai => ai.Insurance)
                .Include(ai => ai.InsuredPerson)
                .FirstOrDefaultAsync(ai => ai.Id == id);

            if (agreedInsurance == null)
                return RedirectToAction("Index");

            return View(agreedInsurance);
        }

        /// <summary>
        /// Zobrazí formulář pro vytvoření nového sjednaného pojištění pro danou osobu.
        /// </summary>
        /// <param name="id">Identifikátor pojištěné osoby.</param>
        [Authorize(Roles = Role.admin + "," + Role.client)]
        public async Task<IActionResult> Create(int id)
        {
            var insurances = await context.Insurances.ToListAsync();
            if (!insurances.Any())
            {
                var userRedirect = await context.Users
                    .Include(x => x.InsuredPerson)
                    .FirstOrDefaultAsync(x => x.InsuredPerson!.Id == id);

                TempData["Message"] = "Nelze přidat pojištění, protože žádné pojištění neexistuje (musí se vytvořit)";
                return RedirectToAction("Index", "InsuredPerson", userRedirect != null
                    ? new { email = userRedirect.Email }
                    : null);
            }

            var model = new AssignedInsuranceCreateViewModel
            {
                EstablishmentDate = DateTime.Now,
                ValidTo = DateTime.Now,
                InsuredPersonId = id,
                Insurances = insurances.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.InsuredObject} - {x.Description}"
                }).ToList()
            };

            var insurancePerson = await context.InsuredPersons
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            ViewData["FullName"] = $"{insurancePerson!.FirstName} {insurancePerson.LastName}";
            ViewData["Email"] = insurancePerson.ApplicationUser?.Email;
            return View(model);
        }

        /// <summary>
        /// Zpracuje odeslání formuláře pro vytvoření sjednaného pojištění:
        /// validuje data, kontroluje duplicitu a uloží nové záznamy.
        /// </summary>
        /// <param name="model">ViewModel s údaji pro nové pojištění.</param>
        /// <param name="fullName">Plné jméno pojištěné osoby pro případ chybné validace.</param>
        [HttpPost]
        [Authorize(Roles = Role.admin + "," + Role.client)]
        public async Task<IActionResult> Create(AssignedInsuranceCreateViewModel model, string? fullName)
        {
            if (!ModelState.IsValid)
            {
                model.Insurances = await context.Insurances
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = $"{x.InsuredObject} - {x.Description}"
                    })
                    .ToListAsync();
                return View(model);
            }

            var agreedInsurance = new AgreedInsurance
            {
                InsuranceId = model.InsuranceId,
                InsuredPersonId = model.InsuredPersonId,
                EstablishmentDate = model.EstablishmentDate,
                ValidTo = model.ValidTo,
                Active = true
            };

            var existingInsurance = await context.AgreedInsurances
                .FirstOrDefaultAsync(x =>
                    x.InsuranceId == model.InsuranceId &&
                    x.InsuredPersonId == model.InsuredPersonId);

            var user = await context.InsuredPersons
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == model.InsuredPersonId);

            if (existingInsurance != null)
            {
                TempData["Message"] = "Sjednané pojištění již existuje!";
                ViewData["FullName"] = fullName;
                ViewData["Email"] = user?.ApplicationUser?.Email;

                model.Insurances = await context.Insurances
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = $"{x.InsuredObject} - {x.Description}"
                    })
                    .ToListAsync();
                return View(model);
            }

            if (user?.ApplicationUser == null)
                return NotFound();

            context.AgreedInsurances.Add(agreedInsurance);
            await context.SaveChangesAsync();

            var insurance = await context.Insurances
                .FirstOrDefaultAsync(i => i.Id == model.InsuranceId);

            TempData["Message"] = $"Sjednané pojištění: {insurance?.Description} pro: {user.FirstName} {user.LastName} přidáno";
            return RedirectToAction("Detail", "InsuredPerson", new { email = user.ApplicationUser.Email });
        }
    }
}
