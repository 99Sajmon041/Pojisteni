using Microsoft.EntityFrameworkCore;
using Pojisteni.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Pojisteni.Services
{
    /// <summary>
    /// Kontext Entity Framework Core pro přístup k databázi aplikace,
    /// rozšiřuje IdentityDbContext pro správu uživatelů a rolí.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Konstruktor přijímající možnosti konfigurace databázového kontextu.
        /// </summary>
        /// <param name="options">Nastavení kontextu, včetně připojovacího řetězce.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Kolekce pojištěných osob v databázi.
        /// </summary>
        public DbSet<InsuredPerson> InsuredPersons { get; set; }

        /// <summary>
        /// Kolekce typů pojištění definovaných v systému.
        /// </summary>
        public DbSet<Insurance> Insurances { get; set; }

        /// <summary>
        /// Kolekce uzavřených smluv o pojištění mezi klienty a druhy pojištění.
        /// </summary>
        public DbSet<AgreedInsurance> AgreedInsurances { get; set; }

        /// <summary>
        /// Kolekce událostí spojených s konkrétními uzavřenými pojištěními.
        /// </summary>
        public DbSet<InsuranceEvent> InsuranceEvents { get; set; }

        /// <summary>
        /// Konfiguruje relační vazby mezi entitami a chování při odstraňování.
        /// </summary>
        /// <param name="modelBuilder">Builder entit pro konfiguraci modelu.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Při smazání uzavřeného pojištění se kaskádově smažou i jeho události.
            modelBuilder.Entity<InsuranceEvent>()
                .HasOne(e => e.AgreedInsurance)
                .WithMany()
                .HasForeignKey(e => e.AgreedInsuranceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Při smazání základního typu pojištění nechej události existovat (Restrict).
            modelBuilder.Entity<InsuranceEvent>()
                .HasOne(e => e.Insurance)
                .WithMany()
                .HasForeignKey(e => e.InsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Při smazání pojištěné osoby nechej události existovat (Restrict).
            modelBuilder.Entity<InsuranceEvent>()
                .HasOne(e => e.InsuredPerson)
                .WithMany()
                .HasForeignKey(e => e.InsuredPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Při smazání pojištěné osoby se kaskádově smažou i její uzavřená pojištění.
            modelBuilder.Entity<AgreedInsurance>()
                .HasOne(x => x.InsuredPerson)
                .WithMany(p => p.Insurances)
                .HasForeignKey(x => x.InsuredPersonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Při smazání typu pojištění se kaskádově smažou příslušné uzavřené pojištění.
            modelBuilder.Entity<AgreedInsurance>()
                .HasOne(x => x.Insurance)
                .WithMany(i => i.InsuredPersonList)
                .HasForeignKey(x => x.InsuranceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}