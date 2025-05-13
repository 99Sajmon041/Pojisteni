using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// Reprezentuje pojištěnou osobu v systému.
    /// </summary>
    public class InsuredPerson
    {
        /// <summary>
        /// Unikátní identifikátor pojištěné osoby.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Křestní jméno pojištěnce (2–15 znaků).
        /// </summary>
        [Required(ErrorMessage = "Musíte zadat jméno pojištěnce")]
        [MinLength(2)]
        [MaxLength(15)]
        public string FirstName { get; set; } = "";

        /// <summary>
        /// Příjmení pojištěnce (2–20 znaků).
        /// </summary>
        [Required(ErrorMessage = "Musíte zadat příjmení pojištěnce")]
        [MinLength(2)]
        [MaxLength(20)]
        public string LastName { get; set; } = "";

        /// <summary>
        /// Adresa trvalého bydliště pojištěnce (max. 100 znaků).
        /// </summary>
        [Required(ErrorMessage = "Musíte zadat adresu pojištěnce")]
        [MaxLength(100)]
        public string Address { get; set; } = "";

        /// <summary>
        /// Datum narození pojištěnce.
        /// </summary>
        [Required(ErrorMessage = "Musíte zadat datum narození pojištěnce")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Kombinace křestního jména a příjmení (např. "Jan Novák").
        /// </summary>
        public string FullName
        {
            get => $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Cizí klíč na odpovídajícího uživatele Identity (AspNetUsers).
        /// </summary>
        public string ApplicationUserId { get; set; } = "";

        /// <summary>
        /// Navigační vlastnost na entitu uživatele rozšířenou o detaily pro pojištěnce.
        /// </summary>
        public ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Seznam všech sjednaných pojištění pro danou osobu.
        /// </summary>
        public List<AgreedInsurance> Insurances { get; set; } = new();
    }
}
