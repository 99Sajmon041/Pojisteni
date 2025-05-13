using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pojisteni.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pojisteni.Services;
using System.ComponentModel;

namespace Pojisteni.Controllers
{
    public class InsuranceEventController : Controller
    {
        private readonly ApplicationDbContext context;

        public InsuranceEventController(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Zobrazí seznam pojistných událostí, případně filtrovaný podle zadaného hledaného řetězce.
        /// </summary>
        /// <param name="search">Volitelný text pro filtrování podle názvu pojištěného objektu nebo jména pojištěné osoby.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Index(string? search)
        {
            List<InsuranceEvent> insuranceEvents;

            if (search != null)
            {
                insuranceEvents = await context.InsuranceEvents
                    .Include(x => x.Insurance)
                    .Include(x => x.AgreedInsurance)
                    .Include(x => x.InsuredPerson)
                    .Where(x => x.Insurance!.InsuredObject.Contains(search)
                             || x.InsuredPerson!.FirstName.Contains(search)
                             || x.InsuredPerson.LastName.Contains(search))
                    .OrderBy(x => x.OccurredOn)
                    .ToListAsync();
            }
            else
            {
                insuranceEvents = await context.InsuranceEvents
                    .Include(x => x.Insurance)
                    .Include(x => x.AgreedInsurance)
                    .Include(x => x.InsuredPerson)
                    .OrderBy(x => x.OccurredOn)
                    .ToListAsync();
            }

            ViewData["SearchEvent"] = search;
            return View(insuranceEvents);
        }

        /// <summary>
        /// Zobrazí detail vybrané pojistné události podle jejího identifikátoru.
        /// </summary>
        /// <param name="id">Id pojistné události.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Detail(int id)
        {
            var insuredEvent = await context.InsuranceEvents
                .Include(x => x.Insurance)
                .Include(x => x.AgreedInsurance)
                .Include(x => x.InsuredPerson)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (insuredEvent == null)
            {
                return RedirectToAction("Index");
            }
            return View(insuredEvent);
        }

        /// <summary>
        /// Odstraní pojistnou událost s daným Id a vrátí zpět na seznam.
        /// </summary>
        /// <param name="id">Id pojistné události k odstranění.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var insuredEvent = await context.InsuranceEvents
                .Include(x => x.Insurance)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (insuredEvent == null)
            {
                return RedirectToAction("Index");
            }

            context.InsuranceEvents.Remove(insuredEvent);
            await context.SaveChangesAsync();
            TempData["Message"] = $"Pojistná událost pro objekt: {insuredEvent.Insurance!.InsuredObject} byla odstraněna";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Zobrazí formulář pro vytvoření nové pojistné události pro danou pojištěnou osobu.
        /// </summary>
        /// <param name="id">Id pojištěné osoby, pro kterou se událost zadává.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Create(int id)
        {
            if (!await context.Insurances.AnyAsync()
                || !await context.AgreedInsurances.AnyAsync(x => x.InsuredPersonId == id))
            {
                var userRedirect = await context.Users
                    .Include(x => x.InsuredPerson)
                    .FirstOrDefaultAsync(x => x.InsuredPerson!.Id == id);

                TempData["Message"] = "Nelze vytvořit událost, protože neexistuje žádné pojištění nebo sjednaná smlouva o pojištění.";
                return RedirectToAction(
                    "Index",
                    "InsuredPerson",
                    userRedirect != null ? new { email = userRedirect.Email } : null);
            }

            var agreedInsurances = await context.AgreedInsurances
                .Include(x => x.Insurance)
                .Where(x => x.InsuredPersonId == id)
                .ToListAsync();

            var user = await context.Users
                .Include(x => x.InsuredPerson)
                .FirstOrDefaultAsync(x => x.InsuredPerson!.Id == id);

            if (user == null || user.InsuredPerson == null)
            {
                return NotFound("Pojištěná osoba nebyla nalezena.");
            }

            string userEmail = user.Email!;
            if (!agreedInsurances.Any())
            {
                // Pokud pro osobu není žádné sjednané pojištění, přesměruj na detail osoby
                return RedirectToAction("Detail", "InsuredPerson", new { email = userEmail });
            }

            var model = new InsuranceEventCreateViewModel
            {
                OccurredOn = DateTime.Today,
                InsuredPersonId = id,
                InsuredPerson = user.InsuredPerson,
                AgreedInsurances = agreedInsurances.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = (x.Insurance!.InsuredObject + " - " + x.Insurance.Description).Length > 50
                       ? (x.Insurance.InsuredObject + " - " + x.Insurance.Description).Substring(0, 47) + "..."
                       : x.Insurance.InsuredObject + " - " + x.Insurance.Description
                }).ToList()
            };

            // Přidá výchozí položku pro výběr
            model.AgreedInsurances.Insert(0, new SelectListItem { Value = "0", Text = "-- Vyberte pojištění --" });
            ViewData["Email"] = userEmail;
            return View(model);
        }

        /// <summary>
        /// Zpracuje odeslaný formulář pro vytvoření pojistné události:
        /// validuje data, uloží novou událost a přesměruje na detail pojištěné osoby.
        /// </summary>
        /// <param name="model">ViewModel s daty nové pojistné události.</param>
        [Authorize(Roles = Role.admin)]
        [HttpPost]
        public async Task<IActionResult> Create(InsuranceEventCreateViewModel model)
        {
            // Validace škody
            if (!decimal.TryParse(model.AmountOfDamage, out decimal amount)
                || amount < 1000 || amount > 10_000_000)
            {
                ModelState.AddModelError(
                    nameof(model.AmountOfDamage),
                    "Neplatná hodnota, ujistěte se, že je zadána částka v rozmezí: 1 000 - 10 000 000");
            }

            // Validace data události
            if (model.OccurredOn > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.OccurredOn),
                    "Datum události nemůže být v budoucnosti.");
            }

            // Validace výběru pojištění
            if (model.AgreedInsuranceId == 0)
            {
                ModelState.AddModelError(
                    nameof(model.AgreedInsuranceId),
                    "Musíte vybrat sjednané pojištění.");
            }

            if (!ModelState.IsValid)
            {
                // Obnova seznamu pro dropdown v případě chyby
                model.AgreedInsurances = await context.AgreedInsurances
                    .Include(x => x.Insurance)
                    .Where(x => x.InsuredPersonId == model.InsuredPersonId)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = (x.Insurance!.InsuredObject + " - " + x.Insurance.Description).Length > 50
                           ? (x.Insurance.InsuredObject + " - " + x.Insurance.Description).Substring(0, 47) + "..."
                           : x.Insurance.InsuredObject + " - " + x.Insurance.Description
                    }).ToListAsync();
                model.AgreedInsurances.Insert(0, new SelectListItem { Value = "0", Text = "-- Vyberte pojištění --" });
                model.InsuredPerson = await context.InsuredPersons
                    .FirstOrDefaultAsync(x => x.Id == model.InsuredPersonId);

                return View(model);
            }

            var agreedInsurance = await context.AgreedInsurances
                .Include(x => x.Insurance)
                .FirstOrDefaultAsync(x => x.Id == model.AgreedInsuranceId);

            if (agreedInsurance == null || agreedInsurance.Insurance == null)
            {
                ModelState.AddModelError("", "Nepodařilo se najít vybrané pojištění.");
                // ...obnovení seznamu a návrat na view (stejně jako výše)
                model.AgreedInsurances = await context.AgreedInsurances
                    .Include(x => x.Insurance)
                    .Where(x => x.InsuredPersonId == model.InsuredPersonId)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = (x.Insurance!.InsuredObject + " - " + x.Insurance.Description).Length > 50
                           ? (x.Insurance.InsuredObject + " - " + x.Insurance.Description).Substring(0, 47) + "..."
                           : x.Insurance.InsuredObject + " - " + x.Insurance.Description
                    }).ToListAsync();
                model.AgreedInsurances.Insert(0, new SelectListItem { Value = "0", Text = "-- Vyberte pojištění --" });
                model.InsuredPerson = await context.InsuredPersons
                    .FirstOrDefaultAsync(x => x.Id == model.InsuredPersonId);

                return View(model);
            }

            var insuranceEvent = new InsuranceEvent
            {
                EventDescription = model.EventDescription,
                AmountOfDamage = amount,
                OccurredOn = model.OccurredOn,
                AgreedInsuranceId = model.AgreedInsuranceId,
                InsuredPersonId = model.InsuredPersonId,
                InsuranceId = agreedInsurance.Insurance.Id
            };

            context.InsuranceEvents.Add(insuranceEvent);
            await context.SaveChangesAsync();
            TempData["Message"] = $"Pojistná událost pro objekt: {insuranceEvent.Insurance!.InsuredObject} byla přidána";

            var insuredPerson = await context.InsuredPersons
                .Include(ip => ip.ApplicationUser)
                .FirstOrDefaultAsync(ip => ip.Id == insuranceEvent.InsuredPersonId);

            string email = insuredPerson!.ApplicationUser!.Email!;
            return RedirectToAction("Detail", "InsuredPerson", new { email });
        }

        /// <summary>
        /// Zobrazí formulář pro úpravu existující pojistné události.
        /// </summary>
        /// <param name="id">Id pojistné události k úpravě.</param>
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var insuredEvent = await context.InsuranceEvents
                .Include(x => x.InsuredPerson)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (insuredEvent == null)
            {
                TempData["Message"] = "Pojistná událost nebyla nalezena";
                return View("Index");
            }

            var InsuredEventModel = new InsuranceEventCreateViewModel
            {
                Id = insuredEvent.Id,
                EventDescription = insuredEvent.EventDescription,
                AmountOfDamage = insuredEvent.AmountOfDamage.ToString(),
                OccurredOn = insuredEvent.OccurredOn,
                AgreedInsuranceId = insuredEvent.AgreedInsuranceId,
                InsuredPersonId = insuredEvent.InsuredPersonId,
                InsuredPerson = insuredEvent.InsuredPerson,
                AgreedInsurances = new List<SelectListItem>()
            };

            return View(InsuredEventModel);
        }

        /// <summary>
        /// Zpracuje odeslaný formulář pro úpravu pojistné události, validuje a uloží změny.
        /// </summary>
        /// <param name="model">ViewModel obsahující upravená data pojistné události.</param>
        [HttpPost]
        [Authorize(Roles = Role.admin)]
        public async Task<IActionResult> Edit(InsuranceEventCreateViewModel model)
        {
            if (!decimal.TryParse(model.AmountOfDamage, out decimal amount)
                || amount < 1000 || amount > 10_000_000)
            {
                ModelState.AddModelError(
                    nameof(model.AmountOfDamage),
                    "Neplatná hodnota, ujistěte se, že je zadána částka v rozmezí: 1 000 - 10 000 000");
            }

            if (model.OccurredOn > DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(model.OccurredOn),
                    "Datum události nemůže být v budoucnosti.");
            }

            if (!ModelState.IsValid)
            {
                model.InsuredPerson = await context.InsuredPersons.FindAsync(model.InsuredPersonId);
                model.AgreedInsurances = new List<SelectListItem>();
                return View(model);
            }

            var insuredEvent = await context.InsuranceEvents.FindAsync(model.Id);
            if (insuredEvent == null)
            {
                TempData["Message"] = "Pojistná událost nebyla nalezena";
                return RedirectToAction("Index");
            }

            insuredEvent.EventDescription = model.EventDescription;
            insuredEvent.AmountOfDamage = amount;
            insuredEvent.OccurredOn = model.OccurredOn;

            await context.SaveChangesAsync();
            TempData["Message"] = "Pojistná událost byla upravena";

            return RedirectToAction("Index");
        }
    }
}
