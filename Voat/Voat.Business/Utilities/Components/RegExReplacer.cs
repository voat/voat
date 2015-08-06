using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Voat.Utilities.Components
{
    public interface IReplacer
    {
        string Replace(string content, object state);
    }

    public class RegExReplacer : IReplacer
    {
        private List<IReplacer> _replacers = new List<IReplacer>();

        public List<IReplacer> Replacers
        {
            get { return this._replacers; }
            set { this._replacers = value; }
        }
        public RegExReplacer(List<IReplacer> Replacers)
        {
            this._replacers = Replacers;
        }
        public RegExReplacer()
        {
        }
        public string Replace(string content, object state)
        {
            foreach (IReplacer ir in _replacers)
            {
                if (content != null)
                {
                    content = ir.Replace(content, state);
                }
            }
            return content;
        }
    }

    public class MatchProcessingReplacer : IReplacer
    {
        #region IReplacer Members
        private string _regex = "";
        private Func<Match, string, object, string> _replacementFunc;
        private int _matchThreshold = 0;
        private bool _ignoreDuplicateMatches = false;

        public bool IgnoreDuplicateMatches
        {
            get { return _ignoreDuplicateMatches; }
            set { _ignoreDuplicateMatches = value; }
        }

        public int MatchThreshold
        {
            get { return _matchThreshold; }
            set { _matchThreshold = value; }
        }

        public MatchProcessingReplacer(string RegEx, Func<Match, string, object, string> Func)
        {
            this.RegEx = RegEx;
            this._replacementFunc = Func;
        }
        public string RegEx
        {
            get { return _regex; }
            set { _regex = value; }
        }
        public bool IsInMarkDownAnchor(Match m, string content)
        {
            var markdownAnchors = Regex.Matches(content, @"\[.*?\]\(.+?\)");
            foreach (Match anchor in markdownAnchors)
            {
                if (m.Index > anchor.Index && m.Index < (anchor.Index + anchor.Length))
                {
                    return true;
                }
            }
            return false;
        }
        public bool HasAnyTokens(string content, params string[] blockTokens)
        {

            foreach (string blockToken in blockTokens)
            {
                if (content.Contains(blockToken))
                {
                    return true;
                }
            }

            return false;

        }
        public bool IsInBlock(Match m, string content, params string[] blockTokens)
        {

            foreach (string blockToken in blockTokens)
            {
                //determine if match is in block
                if (content.IndexOf(blockToken) >= 0)
                {

                    //we have codeblocks in comment
                    int blockIndex = content.IndexOf(blockToken); //find first block start
                    if (m.Index < blockIndex)
                    {
                        //match is before the first block, we continue processing the match
                        return false;
                    }
                    else
                    {

                        int start = blockIndex;
                        while (start >= 0)
                        {

                            int end = content.IndexOf(blockToken, start + 1);
                            if (end >= 0)
                            {
                                if (m.Index > start && m.Index < end)
                                {
                                    return true;
                                }
                                else if (m.Index < start)
                                {
                                    return false;
                                }
                                start = content.IndexOf(blockToken, end + 1);
                            }
                            else
                            {
                                //open codeblock with no end, we bail on this catastrophe of a formatting nightmare
                                break;
                            }
                        }

                    }
                }
            }
            return false;
        }
        public virtual string Replace(string content, object state)
        {
            if (content == null)
            {
                return content;
            }
            MatchCollection matches = Regex.Matches(content, RegEx);
            string result = content;
            string[] escapeBlocks = { "~~~", "`" };


            int offset = 0;
            List<string> matchvalues = new List<string>();
            int maxIndex = (MatchThreshold > 0) ? Math.Min(MatchThreshold, matches.Count) : matches.Count;

            //flag content as having ignored areas if it has more than 1 match 
            bool requiresAdditionalProcecessing = (maxIndex > 0) ? HasAnyTokens(content, escapeBlocks) : false;

            for (int i = 0; i < maxIndex; i++)
            {
                Match m = matches[i];
                //make sure this match isn't in a block
                if (!requiresAdditionalProcecessing || (requiresAdditionalProcecessing && !IsInBlock(m, content, escapeBlocks)))
                {

                    //make sure this match isn't in an anchor
                    if (!IsInMarkDownAnchor(m, content))
                    {

                        if (!IgnoreDuplicateMatches || IgnoreDuplicateMatches && !matchvalues.Contains(m.Value))
                        {

                            //get the replacement value for match 
                            string substitution = _replacementFunc(m, content, state);

                            //Concat method (fractions of milliseconds faster)
                            result = String.Concat(result.Substring(0, m.Index + offset), substitution, result.Substring(m.Index + m.Length + offset, result.Length - (m.Length + m.Index + offset)));
                            //Replace method
                            //result = result.Remove(m.Index + offset, m.Length).Insert(m.Index + offset, substitution);

                            offset += substitution.Length - m.Length;

                            matchvalues.Add(m.Value);

                        }

                    }
                }
            }
            return result;
        }

        #endregion
    }

}