using System;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// ViewModel pro vytváření nebo úpravu údajů o pojištěné osobě.
    /// </summary>
    public class InsuredPersonDetailViewModel
    {
        /// <summary>
        /// Unikátní identifikátor pojištěné osoby (pouze pro editaci).
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
        /// E-mail uživatele, slouží jako uživatelské jméno.
        /// </summary>
        [Required(ErrorMessage = "Zadejte prosím e-mailovou adresu")]
        [EmailAddress(ErrorMessage = "Zadejte e-mailovou adresu v platném formátu")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Heslo pro přihlášení (minimálně 6 znaků).
        /// </summary>
        [Required(ErrorMessage = "Musíte zadat heslo")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků")]
        public string Password { get; set; } = "";
    }
}
