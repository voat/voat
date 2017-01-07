using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Query
{
    
    public class QuerySubmissionSubverseSettings : Query<IEnumerable<Models.SubverseSubmissionSetting>>
    {
        private string _subverseNameLike;
        private bool _exactMatch;

        public QuerySubmissionSubverseSettings(string subverseName, bool exactMatch)
        {
            this._subverseNameLike = subverseName;
            this._exactMatch = exactMatch;
        }

        public override async Task<IEnumerable<Models.SubverseSubmissionSetting>> ExecuteAsync()
        {
            using (var repo = new Data.Repository())
            {
                return await repo.SubverseSubmissionSettingsSearch(_subverseNameLike, _exactMatch);
            }
        }
    }
}
