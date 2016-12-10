using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data
{
    public class DapperQuery
    {
       
        public string Select { get; set; }

        public string SelectColumns { get; set; }

        public string Where { get; set; }

        public string GroupBy { get; set; }

        public string OrderBy { get; set; }

        public object Parameters { get; set; }

        //OFFSET 10 ROWS
        public int? SkipCount { get; set; }

        //FETCH NEXT 10 ROWS ONLY
        public int? TakeCount { get; set; }

        private string EnsureStartsWith(string content, string prefix)
        {
            if (!String.IsNullOrEmpty(content) && !content.ToLower().StartsWith(prefix.ToLower()))
            {
                return $"{prefix} {content}";
            }
            return content;
        }
        private string FormattedSelect
        {
            get
            {
                if (Select.Contains("{0}") && !String.IsNullOrEmpty(SelectColumns))
                {
                    return String.Format(Select, SelectColumns);
                }
                return Select;
            }
        }
        public override string ToString()
        {
            var q = $"{EnsureStartsWith(FormattedSelect, "SELECT ")} {EnsureStartsWith(Where, "WHERE ")} {EnsureStartsWith(GroupBy, "GROUP BY ")} {EnsureStartsWith(OrderBy, "ORDER BY ")}";
            if (TakeCount.HasValue && TakeCount.Value > 0)
            {
                q += String.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (SkipCount.HasValue ? SkipCount.Value : 0).ToString(), TakeCount.Value.ToString());
            }
            return q;
        }
        public string AppendClause(string currentValue, string appendValue, string seperator)
        {
            return currentValue + (String.IsNullOrEmpty(currentValue) ? appendValue : seperator + appendValue);
        }
        public void Append<P>(Expression<Func<DapperQuery, P>> expression, string appendClause)
        {
            var body = (MemberExpression)expression.Body;
            string name = body.Member.Name;

            switch (name.ToLower())
            {
                case "select":
                    Select = AppendClause(Select, appendClause, " ");
                    break;
                case "orderby":
                    OrderBy = AppendClause(OrderBy, appendClause, ", ");
                    break;
                case "where":
                    Where = AppendClause(Where, appendClause, " AND ");
                    break;
                case "groupby":
                    GroupBy = AppendClause(GroupBy, appendClause, ", ");
                    break;
            }
        }
    }
}
