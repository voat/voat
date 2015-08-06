using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.RulesEngine
{

    public class RuleException : ApplicationException {

        public RuleException(string message) : base(message) { }

        public RuleException(string format, params object[] args) : base(String.Format(format, args)) { }

        public RuleException(string message, Exception innerException) : base(message, innerException) { }

    }

}
