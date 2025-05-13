using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// Reprezentuje typ pojištění definovaný v systému.
    /// </summary>
    public class Insurance
    {
        /// <summary>
        /// Unikátní identifikátor pojištění.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Předmět pojištění (věc nebo osoba), na který se pojištění vztahuje.
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
        /// Roční částka platby pojištění v Kč.
        /// </summary>
        [Required(ErrorMessage = "Zadejte částku, na kterou se má pojištění vztahovat")]
        [Range(1, 1_000_000, ErrorMessage = "Částka musí být větší než 0 a menší než 1 milión")]
        [Precision(18, 2)]
        public decimal AnnualPayment { get; set; }

        /// <summary>
        /// Navigační vlastnost – seznam všech sjednaných pojištění tohoto typu pro různé osoby.
        /// </summary>
        public List<AgreedInsurance> InsuredPersonList { get; set; } = new();
    }
}
