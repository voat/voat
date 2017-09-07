using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Utilities;
using Voat.Validation;
using Voat.Voting.Attributes;
using Voat.Voting.Models;

namespace Voat.Voting.Outcomes
{
    [Outcome(Enabled = true, Name = "Remove Moderator", Description = "Remove a moderator from a subverse")]
    public class RemoveModeratorOutcome : ModeratorOutcome, IValidatableObject
    {
        public override async Task<CommandResponse> Execute()
        {
            var m = new Data.Models.SubverseModerator();
            m.UserName = UserName;
            m.Subverse = Subverse;
            
            var cmd = new RemoveModeratorCommand(m);
            var result = await cmd.Execute();
            return result;
        }

        public override string ToDescription()
        {
            return $"@{UserName} well be removed from v/{Subverse}";
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            string originalUserName = UserHelper.OriginalUsername(UserName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                errors.Add(ValidationPathResult.Create(this, $"User '{UserName}' can not be found", x => x.UserName));
            }
            else if (!ModeratorPermission.IsModerator(UserName, Subverse))
            {
                errors.Add(ValidationPathResult.Create(this, $"User '{UserName}' is not a moderator of {Subverse}", x => x.UserName));
            }
            else
            {
                UserName = originalUserName;
            }
            return errors;
        }
    }
}
