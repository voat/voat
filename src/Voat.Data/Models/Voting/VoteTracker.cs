using System;
using System.Collections.Generic;
using System.Text;


namespace Voat.Data.Models
{
    public class VoteTracker
    {
        public int ID { get; set; }
        public int VoteID { get; set; }
        public int VoteOptionID { get; set; }
        public bool RestrictionsPassed { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
