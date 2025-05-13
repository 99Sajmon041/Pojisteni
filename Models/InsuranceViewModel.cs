using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// ViewModel pro vytvoření nebo úpravu druhu pojištění.
    /// </summary>
    public class InsuranceViewModel
    {
        /// <summary>
        /// Unikátní identifikátor pojištění (pouze pro editaci existujícího záznamu).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pojistný objekt (věc nebo osoba), na který se pojištění vztahuje.
        /// </summary>
        [Required(ErrorMessage = "Zadejte pojistnou věc nebo osobu")]
        [MaxLength(100)]
        public string InsuredObject { get; set; } = "";

        /// <summary>
        /// Popis pojištění – čeho se pojištění týká.
        /// </summary>
        [Required(ErrorMessage = "Zadejte popis čeho se pojištění týká")]
        [MaxLength(200)]
        public string Description { get; set; } = "";

        /// <summary>
        /// Roční částka platby pojištění ve formě textu (převedena na <see cref="decimal"/> při zpracování).
        /// </summary>
        [Required(ErrorMessage = "Zadejte hodnotu")]
        public string AnnualPayment { get; set; } = "";
    }
}
