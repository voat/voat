using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Voat.RulesEngine
{

    /// <summary>
    /// Reflection is expensive so this class helps cache any reflection based discovery.
    /// </summary>
    public static class RuleDiscoveryProvider {

        //HACK: This might be overkill but if we are searching by assembly we need to cache by assembly as well
        private static Dictionary<string, List<RuleInformation>> _descriptions = new Dictionary<string, List<RuleInformation>>();
        private static Dictionary<string, List<Tuple<Type, RuleLoadableAttribute>>> _ruleCache = new Dictionary<string, List<Tuple<Type, RuleLoadableAttribute>>>();


        public static List<RuleInformation> GetDescriptions(IEnumerable<Assembly> assemblies) {
            List<RuleInformation> rules = new List<RuleInformation>();
            
            foreach (Assembly assembly in assemblies){
                
                rules.AddRange(GetDescriptions(assembly));

            }
            return rules.OrderBy(x => x.Number).ToList();

        }
        public static List<RuleInformation> GetDescriptions(Assembly discoverAssembly) {

                lock(typeof(RuleDiscoveryProvider)){

                    string cacheKey = discoverAssembly.FullName;

                    if (!_descriptions.ContainsKey(cacheKey)) {

                        _descriptions[cacheKey] = new List<RuleInformation>();

                        var rules = DiscoverRules(new Assembly[] { discoverAssembly });

                        foreach (var rulepair in rules) {
                            
                            if (!rulepair.Item2.Enabled) {
                                continue;
                            }

                            RuleInformation info = new RuleInformation();
                            info.Description = rulepair.Item2.Description;
                            info.PsuedoLogic = rulepair.Item2.PsuedoLogic;

                            Rule rule = null;
                            try {
                                rule = RulesEngine<RuleContext>.ConstructRule(rulepair.Item1);
                            } catch { }
                            if (rule != null) {
                                info.Name = rule.Name;
                                info.Number = rule.Number;
                                info.Scope = rule.Scope;
                            } else {
                                info.Name = "RuleLoadingError";
                                info.Number = "0.0";
                                info.Scope = RuleScope.Global;
                            }

                            _descriptions[cacheKey].Add(info);

                        }


                        _descriptions[cacheKey] = _descriptions[cacheKey].OrderBy(x => x.Number).ToList();

                    }
                    return _descriptions[cacheKey];
                }
        
            

        }

        public static List<Tuple<Type, RuleLoadableAttribute>> DiscoverRules(IEnumerable<Assembly> assemblies) {

            var list = new List<Tuple<Type, RuleLoadableAttribute>>();

            foreach (Assembly assembly in assemblies) {
                string cacheKey = assembly.FullName;

                if (_ruleCache.ContainsKey(cacheKey)) {
                    list.AddRange(_ruleCache[cacheKey]);
                } else {
                    var discovered = DiscoverTypes<RuleLoadableAttribute>(new Assembly[] { assembly });
                    _ruleCache.Add(cacheKey, discovered);
                    list.AddRange(discovered);
                }
            }

            return list;

        }
        public static List<Tuple<Type, T>> DiscoverTypes<T>(IEnumerable<Assembly> assemblies, Type subClassRestriction = null) where T : Attribute {
            
            var allRules = new List<Tuple<Type, T>>();

            if (assemblies != null && assemblies.Count() > 0) {

                foreach (Assembly assembly in assemblies) {
                    var tempRules = new List<Tuple<Type, T>>();
                    if (assembly != null) {
                        var types = assembly.GetTypes();
                        if (types != null && types.Length > 0) {
                            foreach (Type type in types) {

                                //Ensure a rule type
                                if (subClassRestriction == null || (subClassRestriction != null && type.IsSubclassOf(subClassRestriction))) {

                                    var atts = type.GetCustomAttributes<T>();
                                    if (atts != null && atts.Count() > 0) {
                                        foreach (var att in atts) {

                                            tempRules.Add(new Tuple<Type, T>(type, att));

                                        }
                                    }
                                }
                            }
                        }
                    }
                    allRules.AddRange(tempRules);
                }
            }
            return allRules;
        }
    }

    public class RuleInformation {

        public string Name { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string PsuedoLogic { get; set; }
        public RuleScope Scope { get; set; }
        
    }
}