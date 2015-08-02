using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.RulesEngine
{

    public class RuleContext {

        
        private NullFriendlyExpandoObject _expando = new NullFriendlyExpandoObject();

       
        public dynamic PropertyBag {
            get {
                return _expando;
            }
        }
       

        public override string ToString() {

            StringBuilder sb = new StringBuilder();
            short i = 0;
            if (_expando._values.Count > 0) {
                foreach (var key in _expando._values) {
                    sb.AppendFormat(String.Format("{2}{0}: {1}", key.Key, key.Value.ToString(), (i != 0 ? "\n" : "")));
                    i++;
                }
            }
            return sb.ToString();
        }



        private class NullFriendlyExpandoObject : DynamicObject {

            internal readonly Dictionary<string, object> _values = new Dictionary<string, object>();

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                _values.TryGetValue(binder.Name, out result);
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value) {
                _values[binder.Name] = value;
                return true;
            }
        }
    }
}
