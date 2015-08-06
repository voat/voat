using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Voat.Data.Models
{
    public class VoatUser : IdentityUser
    {
        public DateTime RegistrationDateTime { get; set; }
        public DateTime LastLoginDateTime { get; set; }

        [StringLength(50)]
        public string LastLoginFromIp { get; set; }

        // user registered as partner: original content creator - in form of submissions or comments
        public bool Partner { get; set; }
    }

    public class VoatPartner : IdentityUser
    {
        public virtual PartnerInformation PartnerInformation { get; set; }
    }

    // stores information about Voat partners
    public class PartnerInformation
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // possible payment forms: BitCoin, Checks, Electronic Funds Transfer (EFT), EFT via Single Euro Payments Area (SEPA), Wire Transfer and Western Union Quick Cash
        public string PartnerPaymentForm { get; set; }
        public string PartnerBankAccountNumber { get; set; }
        public string PartnerNameOfAccountHolder { get; set; }
        public string PartnerSwiftCode { get; set; }
        public string PartnerBankName { get; set; }
        public string PartnerIFSC { get; set; } // India
        public string PartnerBIK { get; set; } // Russia
        public string PartnerPaymentCurrency { get; set; }
        public string PartnerPhoneNumber { get; set; }
        public string PartnerPayeeName { get; set; }
        public string PartnerCity { get; set; }
        public string PartnerCountry { get; set; }
        public string PartnerZip { get; set; }
        public string PartnerStreet { get; set; }
        public DateTime PartnerLastPaymentDate { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<VoatUser>
    {
        public ApplicationDbContext() : base("voatUsers") { }

        public DbSet<PartnerInformation> PartnerInformation { get; set; }
    }
}
