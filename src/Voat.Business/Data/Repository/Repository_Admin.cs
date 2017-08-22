using Dapper;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Data
{
    public partial class Repository
    {
        [Authorize(Roles = "GlobalAdmin,Admin,DelegateAdmin,GlobalBans")]
        public async Task<CommandResponse> BanGlobally(IEnumerable<GenericReference<BanType>> banList, string reason)
        {
            DemandAuthentication();
            var statements = new List<DapperInsert>();
            
            foreach (var banItem in banList)
            {
                var insert = new DapperInsert();
                string tablename = "", columnname = "";

                insert.Parameters.Add("CreatedBy", User.Identity.Name);
                insert.Parameters.Add("CreationDate", CurrentDate);
                insert.Parameters.Add("Reason", reason);

                switch (banItem.Type)
                {
                    case BanType.Domain:
                        insert.Parameters.Add("Value", banItem.Name.ToNormalized(Normalization.Lower));
                        tablename = "BannedDomain";
                        columnname = "Domain";

                        break;
                    case BanType.User:
                        insert.Parameters.Add("Value", banItem.Name);
                        tablename = "BannedUser";
                        columnname = "UserName";
                        break;
                }

                insert.Insert = SqlFormatter.Table(tablename) + $" (\"{columnname}\", \"Reason\", \"CreatedBy\", \"CreationDate\")";
                insert.Insert += $" SELECT @Value, @Reason, @CreatedBy, @CreationDate WHERE NOT EXISTS (SELECT * FROM {SqlFormatter.Table(tablename)} WHERE \"{columnname}\" = @Value)";
                statements.Add(insert);
            }
            //This needs to executed in a single batch, but since a low traffic scenario this is ok
            foreach (var statement in statements)
            {
                await _db.Connection.ExecuteAsync(statement.ToString(), statement.Parameters);
            }

            return CommandResponse.FromStatus(Status.Success);
        }
    }
}
