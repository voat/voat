using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data
{

    public enum DataStoreType
    {
        SqlServer,
        PostgreSQL
    }

    public class SqlFormatter
    {
        public static string DefaultSchema
        {
            get
            {
                return "dbo";
            }
        }

        public static string Table(string name, string alias = null, string schema = null)
        {
            return Table(name, alias, schema, null);
        }
        public static string Table(string name, string alias = null, string schema = null, params string[] hints)
        {

            var result = Object(name, schema);

            result = result + (!String.IsNullOrEmpty(alias) ? String.Format(" AS {0}", alias) : "");

            switch (DataStore) 
            {
                //Only add hints if sql, should really probably remove this
                case DataStoreType.SqlServer:

                    if (hints != null && hints.Any())
                    {
                        result += $" WITH ({String.Join(", ", hints)})";
                    }
                    break;
            }

            return result;
        }
        public static string Object(string name, string schema = null)
        {
            schema = String.IsNullOrEmpty(schema) ? DefaultSchema : schema;

            var result = String.Format("{0}.{1}", QuoteIndentifier(schema), QuoteIndentifier(name));

            return result;
        }
        public static string BooleanLiteral(bool value)
        {
            var result = "";
            switch (DataStore)
            {
                case DataStoreType.PostgreSQL:
                    result = value ? "True" : "False";
                    break;
                default:
                case DataStoreType.SqlServer:
                    result = value ? "1" : "0";
                    break;
            }
            return result;
        }

        public static string QuoteIndentifier(string name)
        {
            string result = null;
            switch (DataStore)
            {
                case DataStoreType.PostgreSQL:
                    result = String.Format("\"{0}\"", name);
                    break;
                default:
                case DataStoreType.SqlServer:
                    result = String.Format("[{0}]", name);
                    break;
            }
            return result;
        }


        private static DataStoreType _dataStore = DataStoreType.SqlServer;

        public static DataStoreType DataStore
        {
            get
            {
                return _dataStore;
            }
            set
            {
                _dataStore = value;
            }
        }

    }
}
