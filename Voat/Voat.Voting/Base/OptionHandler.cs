using System;
using System.Collections.Generic;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting
{
    public abstract class OptionHandler
    {
        public static OptionHandler Construct(string typeName, string options) //where T : OptionHandler
        {
            try
            {
                var item = (OptionHandler)Activator.CreateInstance(Type.GetType(typeName));
                item.Parse(options);
                return item;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can not contruct: {typeName} with options: '{options}'", ex);
            }
        }
        public abstract void Parse(string json);
    }
    public abstract class OptionHandler<T> : OptionHandler where T : Option
    {
        public override void Parse(string json)
        {
            Options = Option.Deserialize<T>(json);
        }
        public T Options { get; set; }
        public abstract string ToDescription();
    }
}
