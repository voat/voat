using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests
{
    public class TestEnvironmentSettings
    {

        public static string SqlScriptRelativePath { get; set; } = @"..\..\..\SqlScripts\" + Voat.Data.DataStoreType.SqlServer.ToString();
    }
}
