using System.Collections.Generic;
using System.Linq;

namespace Whoaverse.Models.ViewModels
{
    public class SetFrontpageViewModel
    {
        // list of default sets
        public List<Defaultset> DefaultSets { get; set; }

        // list of user subscribed sets
        public IQueryable<Usersetsubscription> UserSets { get; set; }

        // list of top submissions from all sets
        public List<SetSubmission> SubmissionsList { get; set; }
    }
}