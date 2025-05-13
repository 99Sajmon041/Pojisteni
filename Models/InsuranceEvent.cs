using System;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// Reprezentuje pojistnou událost spojenou se sjednaným pojištěním.
    /// </summary>
    public class InsuranceEvent
    {
        /// <summary>
        /// Unikátní identifikátor pojistné události.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Popis události (max. 300 znaků).
        /// </summary>
        [Required]
        [StringLength(300)]
        public string EventDescription { get; set; } = "";

        /// <summary>
        /// Částka škody vzniklé při události (v Kč).
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal AmountOfDamage { get; set; }

        /// <summary>
        /// Datum, kdy k události došlo.
        /// </summary>
        public DateTime OccurredOn { get; set; }

        /// <summary>
        /// Cizí klíč na sjednané pojištění (AgreedInsurance.Id).
        /// </summary>
        public int AgreedInsuranceId { get; set; }

        /// <summary>
        /// Cizí klíč na pojištěnou osobu (InsuredPerson.Id).
        /// </summary>
        public int InsuredPersonId { get; set; }

        /// <summary>
        /// Cizí klíč na typ pojištění (Insurance.Id).
        /// </summary>
        public int InsuranceId { get; set; }

        /// <summary>
        /// Plné jméno pojištěné osoby (z navigační vlastnosti InsuredPerson).
        /// </summary>
        public string FullName => $"{InsuredPerson!.FirstName} {InsuredPerson.LastName}";

        // Navigační vlastnosti

        /// <summary>
        /// Navigační vlastnost na sjednané pojištění, ke kterému událost patří.
        /// </summary>
        public AgreedInsurance? AgreedInsurance { get; set; }

        /// <summary>
        /// Navigační vlastnost na pojištěnou osobu, u které k události došlo.
        /// </summary>
        public InsuredPerson? InsuredPerson { get; set; }

        /// <summary>
        /// Navigační vlastnost na definici typu pojištění.
        /// </summary>
        public Insurance? Insurance { get; set; }
    }
}
