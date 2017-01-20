using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class ReportContentCommand : Command<CommandResponse>
    {
        private int _id;
        private Models.ContentType _contentType;
        private int _reasonID;

        public ReportContentCommand(ContentType contentType, int id, int reasonID)
        {
            this._id = id;
            this._contentType = contentType;
            this._reasonID = reasonID;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository())
            {
                var result = await repo.SaveRuleReport(_contentType, _id, _reasonID);
                return result;
            }
        }
    }
}
