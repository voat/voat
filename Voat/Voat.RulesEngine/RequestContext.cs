using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.RulesEngine
{
    public interface IRequestContext
    {
        dynamic PropertyBag
        {
            get;
        }
        bool Contains(string name, Type type = null, bool tryGetValue = false);
    }
    public class RequestContext : IRequestContext
    {
        private NullFriendlyExpandoObject _expando = null;

        public RequestContext()
        {
            _expando = new NullFriendlyExpandoObject(GetMissingValue);
        }

        public dynamic PropertyBag
        {
            get
            {
                return _expando;
            }
        }

        protected virtual object GetMissingValue(string name)
        {
            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            short i = 0;
            if (_expando._values.Count > 0)
            {
                foreach (var key in _expando._values)
                {
                    sb.AppendFormat(String.Format("{2}{0}: {1}", key.Key, key.Value.ToString(), (i != 0 ? "\n" : "")));
                    i++;
                }
            }
            return sb.ToString();
        }

        public bool Contains(string name, Type type = null, bool tryGetValue = false)
        {
            return _expando.Contains(name, tryGetValue);
        }
        //public object this[string name]
        //{
        //    get {
        //        return _expando[name];
        //    }
        //}
        protected class NullFriendlyExpandoObject : DynamicObject
        {
            internal readonly Dictionary<string, object> _values = new Dictionary<string, object>();
            private Func<string, object> _valueFinder = null;

            public NullFriendlyExpandoObject(Func<string, object> valueFinder)
            {
                _valueFinder = valueFinder;
            }

            internal bool Contains(string name, bool tryGet = false)
            {
                var contains = _values.ContainsKey(name);
                if (!contains && tryGet)
                {
                    var val = _valueFinder(name);
                    if (val != null)
                    {
                        _values[name] = val;
                    }
                    contains = _values.ContainsKey(name);
                }
                return contains;
            }
            protected void TryGetValue(string name)
            {
                if (_valueFinder != null)
                {
                    var result = _valueFinder(name);
                    if (result != null)
                    {
                        //Set the bag value in case needed later
                        _values[name] = result;
                    }
                }
            }
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (Contains(binder.Name, true))
                {
                    _values.TryGetValue(binder.Name, out result);
                    Debug.Print(String.Format("NullFriendly Get {0} ({1})", binder.Name, result));
                    return true;
                }
                //can't find, can't get
                result = null;
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                Debug.Print(String.Format("NullFriendly Set {0} = {1}", binder.Name, value));
                _values[binder.Name] = value;
                return true;
            }
            //public object this[string name]
            //{
            //    get
            //    {
            //        if (Contains(name, true))
            //        {
            //            return _values[name];
            //        }
            //        return null;
            //    }
            //}
        }
    }
}
