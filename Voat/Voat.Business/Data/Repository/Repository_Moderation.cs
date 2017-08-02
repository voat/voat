using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<CommandResponse<Comment>> DistinguishComment(int commentID)
        {
            DemandAuthentication();
            var response = CommandResponse.FromStatus<Comment>(null, Status.Invalid);
            var comment = await this.GetComment(commentID);

            if (comment != null)
            {
                // check to see if request came from comment author
                if (User.Identity.Name == comment.UserName)
                {
                    // check to see if comment author is also sub mod or sub admin for comment sub
                    if (ModeratorPermission.HasPermission(User.Identity.Name, comment.Subverse, ModeratorAction.DistinguishContent))
                    {
                        var m = new DapperMulti();

                        var u = new DapperUpdate();
                        u.Update = $"{SqlFormatter.Table("Comment")} SET \"IsDistinguished\" = {SqlFormatter.ToggleBoolean("\"IsDistinguished\"")}";
                        u.Where = "\"ID\" = @id";
                        u.Parameters.Add("id", commentID);
                        m.Add(u);

                        var s = new DapperQuery();
                        s.Select = $"\"IsDistinguished\" FROM {SqlFormatter.Table("Comment")}";
                        s.Where = "\"ID\" = @id";
                        m.Add(s);

                        response = CommandResponse.FromStatus(comment, Status.Success);
                    }
                    else
                    {
                        response.Message = "User does not have permissions to distinquish content";
                        response.Status = Status.Denied;
                    }
                }
                else
                {
                    response.Message = "User can only distinquish owned content";
                    response.Status = Status.Denied;
                }
            }
            else
            {
                response.Message = "Comment can not be found";
                response.Status = Status.Denied;
            }
            return response;
        }

    }
}
