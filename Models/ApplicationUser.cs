using Microsoft.AspNetCore.Identity;

namespace Pojisteni.Models
{
    /// <summary>
    /// Rozšiřuje IdentityUser o další vlastnosti specifické pro aplikaci pojištění.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Křestní jméno uživatele.
        /// </summary>
        public string Firstname { get; set; } = "";

        /// <summary>
        /// Příjmení uživatele.
        /// </summary>
        public string Lastname { get; set; } = "";

        /// <summary>
        /// Adresa uživatele.
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// Navigační vlastnost na související záznam pojištěné osoby.
        /// </summary>
        public InsuredPerson? InsuredPerson { get; set; }
    }
}
