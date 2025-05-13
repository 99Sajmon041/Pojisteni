using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// ViewModel pro přihlášení uživatele do aplikace.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// E-mailová adresa uživatele sloužící jako přihlašovací jméno.
        /// </summary>
        [Required(ErrorMessage = "Zadejte prosím email")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Heslo uživatele pro ověření identity.
        /// </summary>
        [Required(ErrorMessage = "Zadejte prosím heslo")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        /// <summary>
        /// Určuje, zda si uživatel přeje zůstat přihlášen i po zavření prohlížeče.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
