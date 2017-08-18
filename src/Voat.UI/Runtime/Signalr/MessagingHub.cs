#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Threading.Tasks;

using System.Collections.Generic;
using Voat.Utilities;
using Voat.Common;

namespace Voat
{
    //CORE_PORT: Signalr rewritten for core, this is old code
    /*
    [HubName("messagingHub")]
    public class MessagingHub : Hub
    {
        //private static Dictionary<string, Tuple<string, DateTime>> messageCache;

        public MessagingHub()
        {
            //messageCache = new Dictionary<string,Tuple<string,DateTime>>();
        }

        // send a chat message to all users in a subverse room
        [Microsoft.AspNet.SignalR.Authorize]
        public void SendChatMessage(string id, string message, string access)
        {
            if (message == null)
            {
                return;
            }

            message = message.TrimSafe();

            if (!String.IsNullOrEmpty(id))
            {

                // discard message if it contains unicode
                if (message.ContainsUnicode())
                {
                    return;
                }

                // check if user is banned
                if (UserHelper.IsUserGloballyBanned(Context.User.Identity.Name))
                {
                    // message won't be processed
                    // this is necessary because banning a user from a subverse doesn't kick them from chat
                    return;
                }

               

                //Strip out annoying markdown
                message = ChatMessage.SanitizeInput(message);

                if (!String.IsNullOrEmpty(message))
                {
                    var room = ChatRoom.Find(id);
                    if (room != null)
                    {
                        if (room.IsAccessAllowed(Context.User.Identity.Name, access))
                        {
                            // check if user is banned from room (which is subverse right now too)
                            if (UserHelper.IsUserBannedFromSubverse(Context.User.Identity.Name, room.ID))
                            {
                                // message won't be processed
                                return;
                            }

                            var formattedMessage = Formatting.FormatMessage(message, true, true);

                            var chatMessage = room.CreateMessage(Context.User.Identity.Name, formattedMessage);

                            var context = new Rules.VoatRuleContext(Context.User);
                            context.PropertyBag.ChatMessage = chatMessage;

                            var outcome = Rules.VoatRulesEngine.Instance.EvaluateRuleSet(context, RulesEngine.RuleScope.PostChatMessage);
                            if (outcome.IsAllowed)
                            {
                                ChatHistory.Add(chatMessage);
                                //var htmlEncodedMessage = WebUtility.HtmlEncode(formattedMessage);
                                Clients.Group(room.ID).appendChatMessage(chatMessage.User.DisplayName, chatMessage.Message, chatMessage.CreationDate.ToChatTimeDisplay());
                            }
                        }
                    }
                }
            }
        }

        // add a user to a subverse chat room
        public async Task JoinChat(string id, string access = null)
        {
            var room = ChatRoom.Find(id);
            if (room != null && room.IsAccessAllowed(Context.User.Identity.Name, access))
            {
                // reject join request if user is banned if user is authenticated
                if (Context.User.Identity.IsAuthenticated)
                {
                    if (UserHelper.IsUserBannedFromSubverse(Context.User.Identity.Name, id))
                    {
                        // abort join
                        return;
                    }
                    room.AddUser(Context.User.Identity.Name);
                }

                await Groups.Add(Context.ConnectionId, id);
            }
        }

        // remove a user from a subverse chat room
        public async Task LeaveChat(string id)
        {
            var room = ChatRoom.Find(id);
            if (room != null)
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    //room.RemoveUser(Context.User.Identity.Name);
                }
            }
            await Groups.Remove(Context.ConnectionId, id);
        }
    }
    */
}
