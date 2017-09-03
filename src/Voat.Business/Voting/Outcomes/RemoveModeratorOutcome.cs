using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Utilities;
using Voat.Validation;
using Voat.Voting.Attributes;
using Voat.Voting.Models;

namespace Voat.Voting.Outcomes
{
    [Outcome(Enabled = true, Name = "Remove Moderator Outcome", Description = "Remove a moderator to a subverse")]
    public class RemoveModeratorOutcome : ModeratorOutcome, IValidatableObject
    {
        public override CommandResponse Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Will remove @{UserName} from v/{Subverse}";
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (!UserHelper.UserExists(UserName))
            {
                errors.Add(ValidationPathResult.Create(this, $"User '{UserName}' can not be found", x => x.UserName));
            }
            else if (!ModeratorPermission.IsModerator(UserName, Subverse))
            {
                errors.Add(ValidationPathResult.Create(this, $"User '{UserName}' is not a moderator of {Subverse}", x => x.UserName));
            }
            return errors;
        }
    }
}
