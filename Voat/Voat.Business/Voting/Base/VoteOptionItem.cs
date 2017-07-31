using System;
using System.Collections.Generic;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting
{
    public abstract class VoteOptionItem<T> where T : Option
    {
        public void Parse(string json)
        {
            Options = Option.Parse<T>(json);
        }
        public T Options { get; set; }
        public new abstract string ToString();
    }
}
