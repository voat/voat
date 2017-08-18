using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Voting.Attributes
{
    public abstract class VoteAttribute : Attribute
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
