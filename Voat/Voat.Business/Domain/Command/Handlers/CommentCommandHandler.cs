using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Business.DataAccess.Commands;
using Voat.Data;
using Voat.Data.Models;
using Voat.Rules;
using Voat.RulesEngine;

namespace Voat.Business.DataAccess.Handlers
{ 
    public class CommentCommandHandler : 
        CommandHandler, 
        ICommandHandler<CreateCommentCommand, CommandResponse<Comment>>, 
        ICommandHandler<EditCommentCommand, CommandResponse>,
        ICommandHandler<DeleteCommentCommand, CommandResponse>
    {
        public CommandResponse Execute(DeleteCommentCommand command)
        {
            using (var db = new DataGateway())
            {
                db.DeleteComment(command.CommentID);
                return CommandResponse.Success();
            }
        }

        public CommandResponse Execute(EditCommentCommand command)
        {
            using (var db = new DataGateway())
            {
                db.EditComment(command.CommentID, command.Content);
                return CommandResponse.Success();
            }
        }

        public CommandResponse<Comment> Execute(CreateCommentCommand command)
        {
            using (var db = new DataGateway())
            {
                var comment = db.PostComment(command.SubmissionID, command.ParentCommentID, command.Content);
                return CommandResponse.Success(comment);
            }
        }
    }

}
