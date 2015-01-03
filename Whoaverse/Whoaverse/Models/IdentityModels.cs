/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace Voat.Models
{
    public class WhoaVerseUser : IdentityUser
    {
        public DateTime RegistrationDateTime { get; set; }
        
        // user registered as partner: original content creator - in form of submissions or comments
        public bool Partner { get; set; }       
    }

    public class WhoaVersePartner : IdentityUser
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

    public class ApplicationDbContext : IdentityDbContext<WhoaVerseUser>
    {
        public ApplicationDbContext() : base("whoaverseUsers") { }

        public DbSet<PartnerInformation> PartnerInformation { get; set; }
    }
}