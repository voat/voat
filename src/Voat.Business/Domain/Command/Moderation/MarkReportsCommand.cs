using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class MarkReportsCommand : CacheCommand<CommandResponse>
    {
        private int _id;
        private ContentType _type;
        private string _subverse;

        public MarkReportsCommand(string subverse, ContentType type, int id)
        {
            _subverse = subverse;
            _type = type;
            _id = id;
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.MarkReportsAsReviewed(_subverse, _type, _id);
                return result;
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
        }
    }
}
