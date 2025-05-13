using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pojisteni.Models
{
    /// <summary>
    /// ViewModel pro zobrazení detailu pojištěné osoby včetně jejích sjednaných pojištění a pojistných událostí.
    /// </summary>
    public class InsuredPersonAndInsuraceDetailViewModel
    {
        /// <summary>
        /// Unikátní identifikátor pojištěné osoby.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Křestní jméno pojištěné osoby.
        /// </summary>
        public string FirstName { get; set; } = "";

        /// <summary>
        /// Příjmení pojištěné osoby.
        /// </summary>
        public string LastName { get; set; } = "";

        /// <summary>
        /// Kombinované plné jméno pojištěné osoby (FirstName + LastName).
        /// </summary>
        public string FullName
        {
            get => $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Adresa trvalého bydliště pojištěné osoby.
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// Datum narození pojištěné osoby.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// E-mail přidružený k účtu pojištěné osoby.
        /// </summary>
        [EmailAddress]
        public string Email { get; set; } = "";

        /// <summary>
        /// Seznam sjednaných pojištění této osoby.
        /// </summary>
        public List<AgreedInsurance> AgreedInsurances { get; set; } = new();

        /// <summary>
        /// Seznam pojistných událostí této osoby.
        /// </summary>
        public List<InsuranceEvent> InsuranceEvents { get; set; } = new();
    }
}
