using Pojisteni.Models;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// Reprezentuje sjednané pojištění konkrétní osoby na daný druh pojištění.
    /// </summary>
    public class AgreedInsurance
    {
        /// <summary>
        /// Unikátní identifikátor sjednaného pojištění.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cizí klíč na typ pojištění (Insurance.Id).
        /// </summary>
        public int InsuranceId { get; set; }

        /// <summary>
        /// Cizí klíč na pojištěnou osobu (InsuredPerson.Id).
        /// </summary>
        public int InsuredPersonId { get; set; }

        /// <summary>
        /// Datum vzniku sjednaného pojištění.
        /// </summary>
        [Required(ErrorMessage = "Zadejte datum vzniku pojištění")]
        public DateTime EstablishmentDate { get; set; }

        /// <summary>
        /// Datum zániku sjednaného pojištění.
        /// </summary>
        [Required(ErrorMessage = "Zadejte datum zániku pojištění")]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// Označuje, zda je pojištění stále aktivní.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        // Navigační vlastnosti

        /// <summary>
        /// Navigační vlastnost na definici druhu pojištění.
        /// </summary>
        public Insurance? Insurance { get; set; }

        /// <summary>
        /// Navigační vlastnost na pojištěnou osobu.
        /// </summary>
        public InsuredPerson? InsuredPerson { get; set; }
    }
}
