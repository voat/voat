using System.Diagnostics;
using Voat.RulesEngine;

namespace Voat.Rules
{

    public class RequestContextHandler : IRuleContextHandler<VoatRuleContext>
    {

        public virtual VoatRuleContext Context
        {
            get
            {
                //The default behavior is that a new RuleContext object is constructed for every request
                //that gets issued. 
                if (System.Web.HttpContext.Current != null)
                {
                    var context = System.Web.HttpContext.Current;
                    if (context.Items["RulesContext"] == null)
                    {

                        var c = new VoatRuleContext();

                        Debug.Print("VoatRuleContext being constructed : {0}", c.UserName);

                        context.Items["RulesContext"] = c;

                    }
                    return (VoatRuleContext)context.Items["RulesContext"];
                }
                return new VoatRuleContext();
            }
        }
    }


}