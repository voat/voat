using System;
using System.Text.RegularExpressions;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseStylesheet : CachedQuery<Stylesheet>
    {
        private string _subverse;

        public QuerySubverseStylesheet(string subverse) : base(new CachePolicy(TimeSpan.FromMinutes(10)))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return _subverse;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.SubverseStylesheet(_subverse);
            }
        }

        protected override Stylesheet GetData()
        {
            using (var db = new Repository())
            {
                var css = db.GetSubverseStylesheet(_subverse);
                return new Stylesheet() { Raw = css, Minimized = Minify(css) };
            }
        }

        protected string Minify(string css)
        {
            if (!String.IsNullOrEmpty(css))
            {
                //Credit: http://madskristensen.net/post/Efficient-stylesheet-minification-in-C
                css = Regex.Replace(css, @"[a-zA-Z]+#", "#");
                css = Regex.Replace(css, @"[\n\r]+\s*", string.Empty);
                css = Regex.Replace(css, @"\s+", " ");
                css = Regex.Replace(css, @"\s?([:,;{}])\s?", "$1");
                css = css.Replace(";}", "}");
                css = Regex.Replace(css, @"([\s:]0)(px|pt|%|em)", "$1");

                // Remove comments from CSS
                css = Regex.Replace(css, @"/\*[\d\D]*?\*/", string.Empty);
            }
            return css;
        }
    }
}
