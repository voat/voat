using System;
using System.Collections.Generic;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting
{
    public abstract class VoteOptionItem
    {
        public static VoteOptionItem Construct(string typeName, string options)
        {
            var item = (VoteOptionItem)Activator.CreateInstance(Type.GetType(typeName));
            item.Parse(options);
            return item;
        }
        public abstract void Parse(string json);
    }
    public abstract class VoteOptionItem<T> : VoteOptionItem where T : Option
    {
        public override void Parse(string json)
        {
            Options = Option.Parse<T>(json);
        }
        public T Options { get; set; }
        public new abstract string ToString();
    }
}
