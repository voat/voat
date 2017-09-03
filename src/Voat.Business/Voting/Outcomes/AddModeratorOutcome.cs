using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Utilities;
using Voat.Validation;
using Voat.Voting.Attributes;

namespace Voat.Voting.Outcomes
{
    [Outcome(Enabled = true, Name = "Add Moderator Outcome", Description = "Adds a moderator to a subverse")]
    public class AddModeratorOutcome : ModeratorOutcome, IValidatableObject
    {
        public override CommandResponse Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Will add @{UserName} to v/{Subverse} as a {Level.ToString()}";
        }

        public ModeratorLevel Level { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            if (!UserHelper.UserExists(UserName))
            {
                errors.Add(ValidationPathResult.Create(this, $"User {UserName} can not be found", x => x.UserName));
            }
            return errors;
        }
    }
}
