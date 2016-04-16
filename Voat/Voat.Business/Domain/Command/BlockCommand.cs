using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class BlockCommand : Command, IExcutableCommand<CommandResponse>
    {
        protected DomainType _domainType = DomainType.Subverse;
        protected string _name = null;

        public BlockCommand(DomainType domainType, string name)
        {
            _domainType = domainType;
            _name = name;
        }

        public virtual async Task<CommandResponse> Execute()
        {
            using (var db = new Repository())
            {
                await Task.Run(() => db.Block(_domainType, _name, true));
            }
            return CommandResponse.Success();
        }
    }

    public class UnblockCommand : BlockCommand
    {
        public UnblockCommand(DomainType domainType, string name) : base(domainType, name)
        {
        }

        public override async Task<CommandResponse> Execute()
        {
            using (var db = new Repository())
            {
                await Task.Run(() => db.Block(_domainType, _name, false));
            }
            return CommandResponse.Success();
        }
    }
}
