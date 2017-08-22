using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain.Command
{
    public class GlobalBanCommand : CacheCommand<CommandResponse>
    {
        private IEnumerable<GenericReference<BanType>> _banList = null;
        private string _reason;

        public GlobalBanCommand(IEnumerable<GenericReference<BanType>> banItem, string reason)
        {
            _banList = banItem;
            _reason = reason;
        }
        protected override Task<CommandResponse> ExecuteStage(CommandStage stage)
        {
            var commandResponse = CommandResponse.FromStatus(Status.Success);

            switch (stage)
            {
                case CommandStage.OnAuthorization:
                    if (!User.IsInAnyRole(new[] { UserRole.GlobalAdmin, UserRole.Admin, UserRole.DelegateAdmin, UserRole.GlobalBans }))
                    {
                        commandResponse = CommandResponse.FromStatus(Status.Denied, "Permissions not granted");
                    }
                    break;
                case CommandStage.OnValidation:
                    if (_banList == null || _banList.Count() == 0)
                    {
                        commandResponse = CommandResponse.FromStatus(Status.Invalid, "Banlist can not be null or empty");
                    }
                    else
                    {
                        foreach (var banItem in _banList)
                        {

                            switch (banItem.Type)
                            {
                                case BanType.Domain:
                                    //check full url first
                                    var match = Regex.Match(banItem.Name, CONSTANTS.HTTP_LINK_REGEX, RegexOptions.IgnoreCase);
                                    if (match.Success)
                                    {
                                        banItem.Name = match.Groups["domain"].Value;
                                    }
                                    else
                                    {
                                        //check partial
                                        match = Regex.Match(banItem.Name, CONSTANTS.HOST_AND_PATH_LINK_REGEX, RegexOptions.IgnoreCase);
                                        if (!match.Success)
                                        {
                                            commandResponse = CommandResponse.FromStatus(Status.Invalid, $"Domain {banItem.Name} is not valid");
                                        }
                                        else
                                        {
                                            banItem.Name = match.Groups["domain"].Value;
                                        }
                                    }

                                    break;
                                case BanType.User:
                                    var result = UserDefinition.Parse(banItem.Name);
                                    if (result == null)
                                    {
                                        commandResponse = CommandResponse.FromStatus(Status.Invalid, $"UserName {banItem.Name} is not valid");
                                    }
                                    else
                                    {
                                        var originalName = UserHelper.OriginalUsername(result.Name);
                                        if (String.IsNullOrEmpty(originalName))
                                        {
                                            commandResponse = CommandResponse.FromStatus(Status.Invalid, $"User {banItem.Name} does not exist");
                                        }
                                        banItem.Name = originalName;
                                    }
                                    break;
                            }
                        }

                    }
                    break;
            }

            return Task.FromResult(commandResponse);
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var response = await repo.BanGlobally(_banList, _reason);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
            //throw new NotImplementedException();
        }
    }
}
