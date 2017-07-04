using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Voat.Common;

namespace Voat.Business.Utilities
{
    //Yes this class name is a joke. This represents the upper ceiling of my capabilities in concerns to humor. And this is rather depressing.  
    public static class SpooferProofer
    {

        public static IEnumerable<string> CharacterSwapList(string name, IDictionary<string, string> charSwaps = null, bool reverseGenerate = true, Normalization normalization = Normalization.None)
        {
            List<string> l = new List<string>();

            if (charSwaps == null || !charSwaps.Any())
            {
                //Default list of char swaps
                charSwaps = new Dictionary<string, string>();
                charSwaps.Add("i", "l");
                charSwaps.Add("o", "0");
                //charSwaps.Add("h", "hahaha"); //just to make sure offset swapping does not break
                //charSwaps.Add("heart", "like"); //just to make sure offset swapping does not break
            }
            string allSwapped = name;

            Action<string, string, List<string>> processSwap = new Action<string, string, List<string>>((string1, string2, list) => {

                var userArray = list.ToArray();
                foreach (var username in userArray)
                {
                    var lusername = username.ToNormalized(normalization);
                    if (lusername.Contains(string1.ToNormalized(normalization)))
                    {
                        //Add straight swap (all)
                        list.Add(lusername.Replace(string1.ToNormalized(normalization), string2.ToNormalized(normalization)).ToNormalized(normalization));

                        //replace each individual occurance
                        var matches = Regex.Matches(lusername, string1, normalization == Normalization.None ? RegexOptions.None : RegexOptions.IgnoreCase);
                        if (matches.Count > 1) //If it has 1 match the above line already swapped it
                        {
                            //rolling sub
                            string rollingUserName = lusername;
                            var offset = 0;
                            var substitution = string2;
                            var rollingSwap = lusername;

                            List<Match> reverseProcessing = new List<Match>();

                            foreach (Match m in matches)
                            {
                                reverseProcessing.Add(m);
                                //Concat method (fractions of milliseconds faster)
                                rollingSwap = String.Concat(rollingSwap.Substring(0, m.Index + offset), substitution, rollingSwap.Substring(m.Index + m.Length + offset, rollingSwap.Length - (m.Length + m.Index + offset)));
                                list.Add(rollingSwap.ToNormalized(normalization));
                                offset += substitution.Length - m.Length;

                                var individualSwap = String.Concat(rollingSwap.Substring(0, m.Index), substitution, rollingSwap.Substring(m.Index + m.Length, rollingSwap.Length - (m.Length + m.Index)));
                                list.Add(individualSwap.ToNormalized(normalization));
                            }

                            //Reverse swaps
                            offset = 0;
                            substitution = string2;
                            rollingSwap = lusername;
                            reverseProcessing.Reverse();
                            foreach (Match m in reverseProcessing)
                            {
                                //Concat method (fractions of milliseconds faster)
                                rollingSwap = String.Concat(rollingSwap.Substring(0, m.Index + offset), substitution, rollingSwap.Substring(m.Index + m.Length + offset, rollingSwap.Length - (m.Length + m.Index + offset)));
                                list.Add(rollingSwap.ToNormalized(normalization));
                                //offset += substitution.Length - m.Length;
                            }
                        }
                    }
                }
            });

            l.Add(name.ToNormalized(normalization));

            foreach (var swap in charSwaps)
            {
                //swap key for value
                processSwap(swap.Key, swap.Value, l);

                if (reverseGenerate)
                {
                    //swap value for key
                    processSwap(swap.Value, swap.Key, l);
                }
            }

            return l.Distinct().ToList();
        }
    }
}
