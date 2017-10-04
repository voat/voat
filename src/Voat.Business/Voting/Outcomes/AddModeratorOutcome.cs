using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Utilities;
using Voat.Validation;
using Voat.Voting.Attributes;

namespace Voat.Voting.Outcomes
{
    [Outcome(Enabled = true, Name = "Add Moderator", Description = "Add a moderator to a subverse")]
    public class AddModeratorOutcome : ModeratorOutcome, IValidatableObject
    {
        public override async Task<CommandResponse> Execute()
        {
            var m = new Data.Models.SubverseModerator();
            m.UserName = UserName;
            m.Subverse = Subverse;
            m.Power = (int)Level;
            m.CreatedBy = VoatSettings.Instance.SiteUserName;

            var cmd = new AddModeratorCommand(m);
            var result = await cmd.Execute();
            return result;
        }

        public override string ToDescription()
        {
            return $"@{UserName} will be added as a moderator on v/{Subverse} as {Level.ToString()}";
        }

        public ModeratorLevel Level { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            string originalUserName = UserHelper.OriginalUsername(UserName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                errors.Add(ValidationPathResult.Create(this, $"User '{UserName}' can not be found", x => x.UserName));
            }
            else
            {
                UserName = originalUserName;
            }



            return errors;
        }
    }
}
