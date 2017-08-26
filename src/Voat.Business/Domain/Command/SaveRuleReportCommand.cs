using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SaveRuleReportCommand : Command<CommandResponse>
    {
        private ContentType _type;
        private int _id;
        private int _ruleSetID;

        public SaveRuleReportCommand(ContentType contentType, int id, int ruleSetID)
        {
            _type = contentType;
            _id = id;
            _ruleSetID = ruleSetID;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.SaveRuleReport(_type, _id, _ruleSetID);
                return result;
            }
        }
    }
}
