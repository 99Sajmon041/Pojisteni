using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pojisteni.Models;
using Pojisteni.Services;

namespace Pojisteni.Controllers
{
    [Authorize(Roles = Role.admin)]
    public class InsuranceController : Controller
    {
        private readonly ApplicationDbContext context;

        public InsuranceController(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Zobrazí seznam všech druhů pojištění, případně filtrovaný podle zadaného řetězce.
        /// </summary>
        public async Task<IActionResult> Index(string? search)
        {
            List<Insurance> insuerances = new();

            if (search != null)
            {
                insuerances = await context.Insurances
                    .Where(x => x.InsuredObject.Contains(search))
                    .OrderBy(x => x.InsuredObject)
                    .ToListAsync();
            }
            else
            {
                insuerances = await context.Insurances
                    .OrderBy(x => x.InsuredObject)
                    .ToListAsync();
            }

            ViewData["Search"] = search;
            return View(insuerances);
        }

        /// <summary>
        /// Zobrazí formulář pro vytvoření nového typu pojištění.
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Zpracuje odeslaný formulář pro vytvoření pojištění, ověří data,
        /// uloží nový záznam a přesměruje zpět na seznam.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(InsuranceViewModel insuranceModel)
        {
            decimal payment = 0;
            if (string.IsNullOrWhiteSpace(insuranceModel.AnnualPayment) || !decimal.TryParse(insuranceModel.AnnualPayment, out payment))
            {
                ModelState.AddModelError(nameof(insuranceModel.AnnualPayment), "Nezadal jste správně částku");
            }
            else if (payment < 1000 || payment > 1000000)
            {
                ModelState.AddModelError(nameof(insuranceModel.AnnualPayment), "Zadejte částku v rozmezí 1 000 - 1 000 000,- Kč");
            }

            if (!ModelState.IsValid)
            {
                return View(insuranceModel);
            }

            var insurance = new Insurance()
            {
                InsuredObject = insuranceModel.InsuredObject,
                Description = insuranceModel.Description,
                AnnualPayment = payment
            };

            await context.Insurances.AddAsync(insurance);
            await context.SaveChangesAsync();
            TempData["Message"] = $"Pojištění na: {insurance.InsuredObject} přidáno";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Zobrazí detail vybraného pojištění podle jeho identifikátoru.
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            var insurance = await context.Insurances.FindAsync(id);
            if (insurance == null)
            {
                return RedirectToAction("Index");
            }
            return View(insurance);
        }

        /// <summary>
        /// Odstraní pojištění s daným Id a přesměruje na seznam.
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var insurance = await context.Insurances.FindAsync(id);
            if (insurance == null)
            {
                return RedirectToAction("Index");
            }

            context.Insurances.Remove(insurance);
            await context.SaveChangesAsync();
            TempData["Message"] = $"Pojištění na: {insurance.InsuredObject} odstraněno";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Zobrazí formulář pro úpravu existujícího pojištění.
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var insurance = await context.Insurances.FindAsync(id);
            if (insurance == null)
            {
                return RedirectToAction("Index");
            }

            var model = new InsuranceViewModel()
            {
                Id = insurance.Id,
                InsuredObject = insurance.InsuredObject,
                Description = insurance.Description,
                AnnualPayment = insurance.AnnualPayment.ToString()
            };

            return View(model);
        }

        /// <summary>
        /// Zpracuje odeslaný formulář pro úpravu pojištění, ověří data,
        /// uloží změny a přesměruje na seznam.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(int id, InsuranceViewModel model)
        {
            var insurance = await context.Insurances.FindAsync(id);
            if (insurance == null)
            {
                return RedirectToAction("Index");
            }

            if (!decimal.TryParse(model.AnnualPayment, out decimal amount))
            {
                ModelState.AddModelError(nameof(model.AnnualPayment), "Zadejte prosím platnou částku");
            }
            else if (amount < 1000 || amount > 1000000)
            {
                ModelState.AddModelError(nameof(model.AnnualPayment), "Zadejte částku v rozmezí 1 000 - 1 000 000,- Kč");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            insurance.InsuredObject = model.InsuredObject;
            insurance.Description = model.Description;
            insurance.AnnualPayment = amount;

            await context.SaveChangesAsync();
            TempData["Message"] = $"Pojištění na: {insurance.InsuredObject} upraveno";

            return RedirectToAction("Index");
        }
    }
}
