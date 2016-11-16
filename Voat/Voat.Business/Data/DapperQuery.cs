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

        public string Select { get; set; }

        public string Where { get; set; }

        public string GroupBy { get; set; }

        public string OrderBy { get; set; }

        //OFFSET 10 ROWS
        //FETCH NEXT 10 ROWS ONLY

        //public int? SkipCount { get; set; }

        //public int? TakeCount { get; set; }

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
            return $"{EnsureStartsWith(Select, "SELECT ")} {EnsureStartsWith(Where, "WHERE ")} {EnsureStartsWith(GroupBy, "GROUP BY ")} {EnsureStartsWith(OrderBy, "ORDER BY ")}";
        }
    }
}
