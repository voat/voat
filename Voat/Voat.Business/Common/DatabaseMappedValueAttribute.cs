using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat
{
    /// <summary>
    /// This is a marker flag attribute for enums to info the dev that the enum value is used in database
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class DatabaseMappedValueAttribute : Attribute
    {
    }
}
