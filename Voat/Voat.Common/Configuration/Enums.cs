using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common
{
    public enum DataStoreType
    {
        SqlServer,
        PostgreSql
    }
    public enum Normalization
    {
        None = 0,
        Lower = 1,
        Upper = 2
    }
}
