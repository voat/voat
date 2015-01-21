using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Voat.Utils.Components {
    public interface IReplacer {
        string Replace(string Content, object state);
    }

    public class RegExReplacer : IReplacer {
        private List<IReplacer> _replacers = new List<IReplacer>();

        public List<IReplacer> Replacers {
            get { return this._replacers; }
            set { this._replacers = value; }
        }
        public RegExReplacer(List<IReplacer> Replacers) {
            this._replacers = Replacers;
        }
        public RegExReplacer() {
        }
        public string Replace(string Content, object state) {
            foreach (IReplacer ir in _replacers) {
                Content = ir.Replace(Content, state);
            }
            return Content;
        }
    }

    public class MatchStripper : MatchReplacer {
        private bool _stripwhitespacerepetitions = true;
        private bool _stripreplacementrepetitions = true;

        public bool StripReplacementRepetitions {
            get { return _stripreplacementrepetitions; }
            set { _stripreplacementrepetitions = value; }
        }
        private bool _trim = true;

        public bool Trim {
            get { return _trim; }
            set { _trim = value; }
        }

        public MatchStripper(string ReplacementRegEx, string ReplacementValue) : base(ReplacementRegEx, ReplacementValue) { }

        public bool StripWhitespaceRepetitions {
            get { return _stripwhitespacerepetitions; }
            set { _stripwhitespacerepetitions = value; }
        }

        public override string Replace(string Content, object state) {

            string val = base.Replace(Content, state);
            if (StripWhitespaceRepetitions) {
                val = Regex.Replace(val, "\\s+", " ");
            }
            if (StripReplacementRepetitions && ReplacementValue != " " && !String.IsNullOrEmpty(ReplacementValue)) {
                val = Regex.Replace(val, String.Format("({0})+", ReplacementValue), ReplacementValue);
            }

            return (Trim ? val.Trim() : val);
        }
    }
    public class NullReplacer : IReplacer {
        public string Replace(string Content, object state) {
            return Content;
        }
    }
    public class MatchReplacer : IReplacer {
        #region IReplacer Members
        private string _regex = "";
        private string _replacementvalue = "";

        public MatchReplacer(string ReplacementRegEx, string ReplacementValue) {
            this.ReplacementRegEx = ReplacementRegEx;
            this.ReplacementValue = ReplacementValue;
        }
        public string ReplacementRegEx {
            get { return _regex; }
            set { _regex = value; }
        }
        public string ReplacementValue {
            get { return _replacementvalue; }
            set { _replacementvalue = value; }
        }
        public virtual string Replace(string Content, object state) {
            return Regex.Replace(Content, this.ReplacementRegEx, this.ReplacementValue);
        }

        #endregion
    }
    public class MatchProcessingReplacer : IReplacer {
        #region IReplacer Members
        private string _regex = "";
        private Func<Match, object, string> _func;
        private int _matchThreshold = 0;
        private bool _ignoreDuplicateMatches = false;

        public bool IgnoreDuplicateMatches {
            get { return _ignoreDuplicateMatches; }
            set { _ignoreDuplicateMatches = value; }
        }

        public int MatchThreshold {
            get { return _matchThreshold; }
            set { _matchThreshold = value; }
        }

        public MatchProcessingReplacer(string RegEx, Func<Match, object, string> Func) {
            this.RegEx = RegEx;
            this._func = Func;
        }
        public string RegEx {
            get { return _regex; }
            set { _regex = value; }
        }

        public virtual string Replace(string content, object state) {
            MatchCollection matches = Regex.Matches(content, RegEx);
            string result = content;
            int offset = 0;
            List<string> matchvalues = new List<string>();
            int maxIndex = (MatchThreshold > 0) ? Math.Min(MatchThreshold, matches.Count) : matches.Count;

            for (int i = 0; i < maxIndex; i++ ) {
                Match m = matches[i];
                if (!IgnoreDuplicateMatches || IgnoreDuplicateMatches && !matchvalues.Contains(m.Value)) { 
                    string substitution = _func(m, state);
                    result = result.Remove(m.Index + offset, m.Length).Insert(m.Index + offset, substitution);
                    offset += substitution.Length - m.Length;
                    matchvalues.Add(m.Value);
                }
            }
            return result;
        }

        #endregion
    }
    public class DateReplacer : IReplacer {
        private string regex = @"\[[dD]{1}[aA]{1}[tT]{1}[eE]{1}:(?<format>[a-zA-Z_0-9\.-]+)\]";

        public string Replace(string Content, object state) {
            string val = Content.ToString();
            MatchCollection col = Regex.Matches(val, regex);
            if (col.Count > 0) {
                foreach (Match match in col) {
                    string cmd = match.Value;
                    string format = match.Groups[1].Value;

                    val = val.Remove(match.Index, match.Length);
                    //format
                    DateTime dt = DateTime.Now;
                    format = Regex.Replace(format, "([mM]{1}[nN]{1})", dt.Minute.ToString());
                    format = Regex.Replace(format, "[mM]{2}", dt.Month.ToString().PadLeft(2, '0'));
                    format = Regex.Replace(format, "[mM]{1}", dt.Month.ToString());
                    format = Regex.Replace(format, "([dD]{2})", dt.Day.ToString().PadLeft(2, '0'));
                    format = Regex.Replace(format, "([dD]{1})", dt.Day.ToString());
                    format = Regex.Replace(format, "([yY]{4})", dt.Year.ToString());
                    format = Regex.Replace(format, "([yY]{2})", dt.Year.ToString().Substring(2, 2));
                    format = Regex.Replace(format, "([yY]{1})", dt.Year.ToString());
                    format = Regex.Replace(format, "([hH]{1,2})", dt.Hour.ToString());
                    format = Regex.Replace(format, "([sS]{1,2})", dt.Second.ToString());
                    val = val.Insert(match.Index, format);
                }
            }
            return val;
        }
    }
   
}