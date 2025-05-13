using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// ViewModel pro vytvoření nového sjednaného pojištění pro danou osobu.
    /// </summary>
    public class AssignedInsuranceCreateViewModel
    {
        /// <summary>
        /// Id vybraného druhu pojištění.
        /// </summary>
        public int InsuranceId { get; set; }

        /// <summary>
        /// Id pojištěné osoby, pro kterou se pojištění sjednává.
        /// </summary>
        public int InsuredPersonId { get; set; }

        /// <summary>
        /// Datum vzniku sjednaného pojištění.
        /// </summary>
        [Required(ErrorMessage = "Zadejte datum vzniku pojištění")]
        [DataType(DataType.Date)]
        public DateTime EstablishmentDate { get; set; }

        /// <summary>
        /// Datum zániku sjednaného pojištění.
        /// </summary>
        [Required(ErrorMessage = "Zadejte datum zániku pojištění")]
        [DataType(DataType.Date)]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// Seznam dostupných druhů pojištění pro výběr ve formuláři.
        /// </summary>
        public List<SelectListItem> Insurances { get; set; } = new();
    }
}
