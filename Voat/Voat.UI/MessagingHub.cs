/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using System.Collections.Generic;
using Voat.Utilities;

namespace Voat
{
    [HubName("messagingHub")]
    public class MessagingHub : Hub
    {
        //private static Dictionary<string, Tuple<string, DateTime>> messageCache;
       

        public MessagingHub()
        {
            //messageCache = new Dictionary<string,Tuple<string,DateTime>>();
        }

        // send a chat message to all users in a subverse room
        [Authorize]
        public void SendChatMessage(string subverseName, string message)
        {
            if (message == null)
            {
                return;
            }

            message = message.TrimSafe();

            if (!String.IsNullOrEmpty(subverseName))
            {
                // check if user is banned
                if (UserHelper.IsUserGloballyBanned(Context.User.Identity.Name))
                {
                    // message won't be processed
                    // this is necessary because banning a user from a subverse doesn't kick them from chat
                    return;
                }

                // discard message if it contains unicode
                if (Formatting.ContainsUnicode(message))
                {
                    return;
                }

                //Strip out annoying markdown
                message = ChatMessage.SanitizeInput(message);

                if (!String.IsNullOrEmpty(message))
                {
                    var formattedMessage = Formatting.FormatMessage(message, true, true);

                    var chatMessage = new ChatMessage()
                    {
                        RoomName = subverseName,
                        UserName = Context.User.Identity.Name,
                        Message = formattedMessage,
                        CreationDate = Data.Repository.CurrentDate
                    };

                    var context = new Rules.VoatRuleContext();
                    context.PropertyBag.ChatMessage = chatMessage;

                    var outcome = Rules.VoatRulesEngine.Instance.EvaluateRuleSet(context, RulesEngine.RuleScope.PostChatMessage);
                    if (outcome.IsAllowed)
                    {
                        ChatHistory.Add(chatMessage);

                        //var htmlEncodedMessage = WebUtility.HtmlEncode(formattedMessage);
                        Clients.Group(subverseName).appendChatMessage(chatMessage.UserName, formattedMessage, chatMessage.CreationDate.ToChatTimeDisplay());
                    }
                }
            }
        }

        // add a user to a subverse chat room
        public async Task JoinChat(string subverseName)
        {
            // reject join request if user is banned if user is authenticated
            if (Context.User.Identity.IsAuthenticated)
            {
                if (UserHelper.IsUserBannedFromSubverse(Context.User.Identity.Name, subverseName))
                {
                    // abort join
                    return;
                }
            }            

            await Groups.Add(Context.ConnectionId, subverseName);
        }

        // remove a user from a subverse chat room
        public async Task LeaveChat(string subverseName)
        {
            await Groups.Remove(Context.ConnectionId, subverseName);
        }
    }
}
