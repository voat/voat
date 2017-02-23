using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data
{
    public static class DapperExtensions
    {
        public static DynamicParameters ToDynamicParameters(this object parameters)
        {
            var d = new DynamicParameters();
            d.AddDynamicParams(parameters);
            return d;
        }
    }
    /// <summary>
    /// A super simple text based wrapper around query construction used for Dapper 
    /// </summary>
    public class DapperQuery : DapperBase
    {
        public string Select { get; set; }

        public string SelectColumns { get; set; }

        public string GroupBy { get; set; }

        public string Having { get; set; }

        public string OrderBy { get; set; }

        protected string FormattedSelect
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

        //OFFSET 10 ROWS
        public int? SkipCount { get; set; }

        //FETCH NEXT 10 ROWS ONLY
        public int? TakeCount { get; set; }
        public override string ToString()
        {
            var q = $"{EnsureStartsWith(FormattedSelect, "SELECT ")} {EnsureStartsWith(Where, "WHERE ")} {EnsureStartsWith(GroupBy, "GROUP BY ")} {EnsureStartsWith(Having, "HAVING ")} {EnsureStartsWith(OrderBy, "ORDER BY ")}";
            if (TakeCount.HasValue && TakeCount.Value > 0)
            {
                q += String.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (SkipCount.HasValue ? SkipCount.Value : 0).ToString(), TakeCount.Value.ToString());
            }
            return q;
        }

        public override void Append<P>(Expression<Func<DapperQuery, P>> expression, string appendClause)
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
                case "having":
                    Having = AppendClause(Having, appendClause, " AND ");
                    break;
            }
        }
    }
    public class DapperUpdate : DapperBase
    {
        public string Update { get; set; }

        public override string ToString()
        {
            var q = $"{EnsureStartsWith(Update, "UPDATE ")} {EnsureStartsWith(Where, "WHERE ")}";
            return q;
        }
    }
    public class DapperDelete : DapperBase
    {
        public string Delete { get; set; }

        public override string ToString()
        {
            var q = $"{EnsureStartsWith(Delete, "DELETE ")} {EnsureStartsWith(Where, "WHERE ")}";
            return q;
        }
    }
    public class DapperBase
    {
        private DynamicParameters _params = null;

        public string Where { get; set; }

        public DynamicParameters Parameters
        {
            get
            {
                if (_params == null)
                {
                    _params = new DynamicParameters();
                }
                return _params;
            }
            set
            {
                _params = value;
            }
        }

        protected string EnsureStartsWith(string content, string prefix)
        {
            if (!String.IsNullOrEmpty(content) && !content.ToLower().StartsWith(prefix.ToLower()))
            {
                return $"{prefix} {content}";
            }
            return content;
        }

        public string AppendClause(string currentValue, string appendValue, string seperator)
        {
            if (!String.IsNullOrEmpty(appendValue))
            {
                return currentValue + (String.IsNullOrEmpty(currentValue) ? appendValue : seperator + appendValue);
            }
            return currentValue;
        }
        public override string ToString()
        {
            throw new NotImplementedException("This method must be overridden in derived classes");
        }

        public virtual void Append<P>(Expression<Func<DapperQuery, P>> expression, string appendClause)
        {
            var body = (MemberExpression)expression.Body;
            string name = body.Member.Name;

            switch (name.ToLower())
            {
                case "where":
                    Where = AppendClause(Where, appendClause, " AND ");
                    break;
            }
        }
    }
}
