#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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
