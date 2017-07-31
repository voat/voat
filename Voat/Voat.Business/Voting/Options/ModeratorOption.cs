using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Models;

namespace Voat.Voting.Options
{
    public enum ModeratorAction
    {
        Add = 1,
        Remove = 2
    }
    public class ModeratorOption : OutcomeOption
    {
        public string UserName {get; set;}

        public string Subverse {get; set;}

        public ModeratorAction Action {get; set;}

        public ModeratorLevel Level {get; set;}
    }
}
