using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common.Configuration
{
    public class HandlerInfo
    {
        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Arguments { get; set; }

        public T Construct<T>()
        {
            var type = System.Type.GetType(this.Type);
            if (type != null)
            {
                return Construct<T>(type, Arguments);
            }
            throw new InvalidOperationException(String.Format("Can not find type: {0}", this.Type));
        }
        public static T Construct<T>(Type type, string arguments)
        {
            T t = default(T);
            if (type != null)
            {
                object[] args = ArgumentParser.Parse(arguments);
                t = (T)Activator.CreateInstance(type, args);
            }
            return t;
        }
    }
}
