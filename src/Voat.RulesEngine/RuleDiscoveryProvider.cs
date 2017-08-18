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
using System.Linq;
using System.Reflection;
using Voat.Common;

namespace Voat.RulesEngine
{
    /// <summary>
    /// Reflection is expensive so this class helps cache any reflection based discovery.
    /// </summary>
    public static class RuleDiscoveryProvider
    {
        //HACK: This might be overkill but if we are searching by assembly we need to cache by assembly as well
        private static Dictionary<string, List<RuleInformation>> _ruleCache = new Dictionary<string, List<RuleInformation>>();

        public static Dictionary<string, List<RuleInformation>> RuleCache
        {
            get
            {
                return _ruleCache;
            }
        }

        public static List<RuleInformation> GetDescriptions(IEnumerable<Assembly> assemblies)
        {
            List<RuleInformation> rules = new List<RuleInformation>();

            foreach (Assembly assembly in assemblies)
            {
                rules.AddRange(GetDescriptions(assembly));
            }
            return rules.OrderBy(x => x.Rule.Number).ToList();
        }

        public static List<RuleInformation> GetDescriptions(Assembly discoverAssembly)
        {
            lock (typeof(RuleDiscoveryProvider))
            {
                string cacheKey = discoverAssembly.FullName;

                if (!_ruleCache.ContainsKey(cacheKey))
                {
                    var list = new List<RuleInformation>();

                    var rules = DiscoverRules(new Assembly[] { discoverAssembly });

                    foreach (var ruleinfo in rules)
                    {
                        //RuleInformation info = Map(rulepair);
                        list.Add(ruleinfo);
                    }

                    _ruleCache[cacheKey] = list.OrderBy(x => x.Rule.Number).ToList();
                }
                return _ruleCache[cacheKey];
            }
        }

        public static RuleInformation Map(DiscoveredType<RuleDiscoveryAttribute> discoveredRule)
        {
            RuleInformation info = new RuleInformation();
            if (discoveredRule != null)
            {
                info.Description = discoveredRule.Attribute.Description;
                info.PsuedoLogic = discoveredRule.Attribute.PsuedoLogic;
                info.Enabled = discoveredRule.Attribute.Enabled;
            }

            Rule rule = RulesEngine<RequestContext>.ConstructRule(discoveredRule.Type, false);
            info.Rule = rule;
            //if (rule != null)
            //{
            //    info.Name = rule.Name;
            //    info.Number = rule.Number;
            //    info.Scope = rule.Scope;
            //}
            return info;
        }

        public static List<RuleInformation> DiscoverRules(IEnumerable<Assembly> assemblies)
        {
            var list = new List<RuleInformation>();

            foreach (Assembly assembly in assemblies)
            {
                string cacheKey = assembly.FullName;

                if (_ruleCache.ContainsKey(cacheKey))
                {
                    //pull from cache
                    list.AddRange(_ruleCache[cacheKey]);
                }
                else
                {
                    //discover
                    var discovered = TypeDiscovery.DiscoverTypes<RuleDiscoveryAttribute>(new Assembly[] { assembly });

                    foreach (var rule in discovered)
                    {
                        list.Add(Map(rule));
                    }
                    
                    //add to cache
                    _ruleCache[cacheKey] = list;

                }
            }
            return list;
        }
    }

    public class RuleInformation
    {
        public Rule Rule { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }
        public string PsuedoLogic { get; set; }
        //public RuleScope Scope { get; set; }
        //public string Name { get; set; }
        //public string Number { get; set; }
    }
}
