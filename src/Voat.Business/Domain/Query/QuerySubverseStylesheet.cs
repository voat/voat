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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseStylesheet : CachedQuery<Stylesheet>
    {
        private string _subverse;

        public QuerySubverseStylesheet(string subverse) : this(subverse, new CachePolicy(TimeSpan.FromMinutes(10)))
        {
        }
        public QuerySubverseStylesheet(string subverse, CachePolicy policy) : base(policy)
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

        protected override async Task<Stylesheet> GetData()
        {
            using (var db = new Repository(User))
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
