using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data
{
    public class DapperQuery
    {
        public object Parameters { get; set; }

        public string SelectClause { get; set; }

        public string WhereClause { get; set; }

        public string GroupByClause { get; set; }

        public string OrderByClause { get; set; }


        private string EnsureStartsWith(string content, string prefix)
        {
            if (!String.IsNullOrEmpty(content) && !content.ToLower().StartsWith(prefix.ToLower()))
            {
                return $"{prefix} {content}";
            }
            return content;
        }

        public override string ToString()
        {
            return $"{EnsureStartsWith(SelectClause, "SELECT ")} {EnsureStartsWith(WhereClause, "WHERE ")} {EnsureStartsWith(GroupByClause, "GROUP BY ")} {EnsureStartsWith(OrderByClause, "ORDER BY ")}";
        }
    }
}
