using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.RulesEngine {

    public interface IRuleContextHandler<T> where T : RuleContext {

        T Context { get; }

    }

}
