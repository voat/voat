using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Data.Models
{
    public class VoteRestriction
    {
        public int ID { get; set; }
        public int VoteID { get; set; }
        //public string GroupName { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}