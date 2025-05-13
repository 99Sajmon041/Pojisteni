using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// ViewModel pro vytváření či úpravu pojistné události.
    /// </summary>
    public class InsuranceEventCreateViewModel
    {
        /// <summary>
        /// Id pojistné události (pouze pro editaci).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Popis pojistné události.
        /// </summary>
        [Required(ErrorMessage = "Zadejte popis pojistné události")]
        [StringLength(300)]
        public string EventDescription { get; set; } = "";

        /// <summary>
        /// Částka škody ve formě textu (převedena na <see cref="decimal"/> až při zpracování).
        /// </summary>
        [Required(ErrorMessage = "Zadejte hodnotu")]
        public string AmountOfDamage { get; set; } = "";

        /// <summary>
        /// Datum, kdy k události došlo.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime OccurredOn { get; set; }

        /// <summary>
        /// Id sjednaného pojištění, ke kterému se událost vztahuje.
        /// </summary>
        public int AgreedInsuranceId { get; set; }

        /// <summary>
        /// Id pojištěné osoby, u které k události došlo.
        /// </summary>
        public int InsuredPersonId { get; set; }

        /// <summary>
        /// Navigační vlastnost obsahující data pojištěné osoby.
        /// </summary>
        public InsuredPerson? InsuredPerson { get; set; }

        /// <summary>
        /// Seznam dostupných sjednaných pojištění pro výběr v dropdownu.
        /// </summary>
        public List<SelectListItem> AgreedInsurances { get; set; } = new();
    }
}
