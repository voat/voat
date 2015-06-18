namespace Voat.Models
{
    using System.Data.Common;

    public partial class whoaverseEntities
    {
        public whoaverseEntities(DbConnection connection) : base(connection, true)
        {
        }
    }
}