using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class SubverseBan
    {
        public int ID { get; set; }
        public string Subverse { get; set; }
        public string UserName { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string Reason { get; set; }

        //public int SubmissionID { get; set; }
        //public virtual Subverse Subverse1 { get; set; }
    }
}
