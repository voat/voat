using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Data.Models
{
    public class VoteOutcome
    {
        public int ID { get; set; }
        //public int VoteID { get; set; }
        public int VoteOptionID { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
