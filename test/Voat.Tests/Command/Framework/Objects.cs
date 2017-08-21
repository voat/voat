using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Command;

namespace Voat.Tests.CommandTests.Framework
{
    public class TestCommand : Command<CommandResponse>
    {
        private CommandStage? _stage;
        private bool _pass;

        public TestCommand(CommandStage? stage, bool pass = true)
        {
            _stage = stage;
            _pass = pass;
        }
        public CommandStage SetComandStageMask { set => CommandStageMask = value; }
        protected override Task<CommandResponse> ExecuteStage(CommandStage stage)
        {
            var r = new CommandResponse(Status.Success, "");

            if (_stage.HasValue && stage == _stage.Value && !_pass)
            {
                r = CommandResponse.FromStatus(Status.Error, stage.ToString());
            }
            return Task.FromResult(r);
        }

        protected override Task<CommandResponse> ProtectedExecute()
        {
            return Task.FromResult(CommandResponse.FromStatus(Status.Success, ""));
        }
    }
}
