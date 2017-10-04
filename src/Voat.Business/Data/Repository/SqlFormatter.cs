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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Data
{
    public class SqlFormatter
    {
        public static string DefaultSchema
        {
            get
            {
                switch (DataConfigurationSettings.Instance.StoreType)
                {
                    case DataStoreType.SqlServer:
                        return "dbo";
                        break;
                    case DataStoreType.PostgreSql:
                        //will be public when we remove dbo schema from pg files
                        return "public";
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        //public static string IfExists(bool exists, string existsClause, string trueClause, string falseClause = null)
        //{
        //    var result = new StringBuilder();
        //    var existNegation = exists ? " " : " NOT ";
        //    switch (Configuration.Settings.DataStore)
        //    {
        //        case DataStoreType.SqlServer:
        //            result.AppendLine($"IF{existNegation}EXISTS ({existsClause})");
        //            result.AppendLine(trueClause);
        //            if (!String.IsNullOrEmpty(falseClause))
        //            {
        //                result.AppendLine($"ELSE");
        //                result.AppendLine(falseClause);
        //            }
        //            break;
        //        case DataStoreType.PostgreSQL:
        //            result.AppendLine("load 'plpgsql';");
        //            result.AppendLine("DO");
        //            result.AppendLine("$$");
        //            result.AppendLine("BEGIN");

        //            result.AppendLine($"IF{existNegation}EXISTS ({existsClause}) THEN");
        //            result.AppendLine("");
        //            result.AppendLine(trueClause);
        //            if (!String.IsNullOrEmpty(falseClause))
        //            {
        //                result.AppendLine($"ELSE");
        //                result.AppendLine(falseClause);
        //            }
        //            result.AppendLine("END IF;");
        //            result.AppendLine("END;");
        //            result.AppendLine("$$");

        //            break;
        //        default:
        //            throw new NotImplementedException();
        //    }


        //    return result.ToString();
        //}
        public static string OrderBy(string columnName, bool ascending)
        {
            return QuoteIndentifier(columnName) + (ascending ? " ASC" : " DESC");
        }

        public static string DeleteBlock(string fromTable, string alias = null)
        {
            var result = "";

            string aliasClause = String.IsNullOrEmpty(alias) ? " " : $" AS {alias} ";

            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.SqlServer:
                    result = $"DELETE {alias} FROM {fromTable}{aliasClause}";
                    break;
                case DataStoreType.PostgreSql:
                    result = result = $"DELETE FROM {fromTable}{aliasClause}";
                    break;
                default:
                    throw new NotImplementedException();
            }


            return result;
        }
        public static string UpdateSetBlock(string setStatements, string fromTable, string alias = null)
        {
            var result = "";

            string aliasClause = String.IsNullOrEmpty(alias) ? " " : $" AS {alias} ";

            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.SqlServer:

                    if (String.IsNullOrEmpty(alias))
                    {
                        result = $"UPDATE {fromTable} SET {setStatements}";
                    }
                    else
                    {
                        result = $"UPDATE {alias} SET {setStatements} FROM {fromTable}{aliasClause}";
                    }

                    break;
                case DataStoreType.PostgreSql:
                    result = result = $"UPDATE {fromTable}{aliasClause}SET {setStatements}";
                    break;
                default:
                    throw new NotImplementedException();
            }


            return result;
        }
        public static string In(string parameter)
        {
            var result = "";

            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.SqlServer:
                    result = $"IN {parameter}";
                    break;
                case DataStoreType.PostgreSql:
                    result = $"= ANY({parameter})";
                    break;
                default:
                    throw new NotImplementedException();
            }


            return result;
        }

        public static string IsNull(string parameter, string defaultValue)
        {
            var result = "";

            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.SqlServer:
                    result = $"ISNULL({parameter}, {defaultValue})";
                    break;
                case DataStoreType.PostgreSql:
                    result = $"CASE WHEN {parameter} IS NULL THEN {defaultValue} ELSE {parameter} END";
                    break;
                default:
                    throw new NotImplementedException();
            }


            return result;
        }

        public static string Table(string name, string alias = null, string schema = null)
        {
            return Table(name, alias, schema, null);
        }
        public static string Table(string name, string alias = null, string schema = null, params string[] hints)
        {

            var result = Object(name, schema);

            result = result + (!String.IsNullOrEmpty(alias) ? String.Format(" AS {0}", alias) : "");

            switch (DataConfigurationSettings.Instance.StoreType) 
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
            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.PostgreSql:
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
            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.PostgreSql:
                    result = String.Format("\"{0}\"", name);
                    break;
                default:
                case DataStoreType.SqlServer:
                    result = String.Format("[{0}]", name);
                    break;
            }
            return result;
        }
        public static string ToNormalized(string value, Normalization normalization, string alias = null)
        {
            var result = value;

            switch (normalization)
            {
                case Normalization.Lower:
                    switch (DataConfigurationSettings.Instance.StoreType)
                    {
                        case DataStoreType.PostgreSql:
                        case DataStoreType.SqlServer:
                            result = $"lower({value})";
                            break;
                    }
                    break;
                case Normalization.Upper:
                    switch (DataConfigurationSettings.Instance.StoreType)
                    {
                        case DataStoreType.PostgreSql:
                        case DataStoreType.SqlServer:
                            result = $"upper({value})";
                            break;
                    }
                    break;
            }
            if (!String.IsNullOrEmpty(alias))
            {
                result = As(result, alias);
            }
            return result;
        }
        public static string As(string value, string alias)
        {
            return $"{value} AS {alias}";
        }
        public static string ToggleBoolean(string name)
        {
            string result = $"CASE {name} WHEN 0 THEN 1 ELSE 0 END";
            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.PostgreSql:
                    result = $"NOT {name}";
                //case DataStoreType.SqlServer:
                //    result = $"lower({value})";
                    break;
            }
            return result;
        }
    }
}
