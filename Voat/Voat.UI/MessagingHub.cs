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
        private static Dictionary<string, Tuple<string, DateTime>> messageCache;

        public MessagingHub()
        {
            messageCache = new Dictionary<string,Tuple<string,DateTime>>();
        }

        // send a chat message to all users in a subverse room
        [Authorize]
        public void SendChatMessage(string name, string message, string subverseName)
        {
            if (message == null)
            {
                return;
            }

            message = message.Trim();

            if (!String.IsNullOrEmpty(name) && message != String.Empty && !String.IsNullOrEmpty(subverseName))
            {
                // check if user is banned
                if (UserHelper.IsUserBannedFromSubverse(Context.User.Identity.Name, subverseName))
                {
                    // message won't be processed
                    // this is necessary because banning a user from a subverse doesn't kick them from chat
                    return;
                }

                // discard message if it contains unicode
                if (Submissions.ContainsUnicode(message))
                {
                    return;
                }

                // trim message to 200 characters
                if (message.Length > 200)
                {
                    message = message.Substring(0, 200);
                }

                // check if previous message from this user is in cache
                //if (messageCache.ContainsKey(name))
                //{
                //    // discard duplicate message and update timestamp
                //    if (message == messageCache[name].Item1)
                //    {
                //        messageCache.Remove(name);
                //        messageCache.Add(name, new Tuple<string, DateTime>(message, DateTime.UtcNow));
                //        return;
                //    }

                //    // check timestamp and discard if diff less than 5 seconds
                //    var timestamp = messageCache[name].Item2;
                //    if (timestamp.AddSeconds(5) < DateTime.UtcNow)
                //    {
                //        return;
                //    }
                //}

                messageCache.Add(name, new Tuple<string, DateTime>(message, DateTime.UtcNow));
                var htmlEncodedMessage = WebUtility.HtmlEncode(message);
                Clients.Group(subverseName).appendChatMessage(Context.User.Identity.Name, htmlEncodedMessage);
            }
        }

        // add a user to a subverse chat room
        public Task JoinSubverseChatRoom(string subverseName)
        {
            // reject join request if user is banned if user is authenticated
            if (Context.User.Identity.IsAuthenticated)
            {
                if (UserHelper.IsUserBannedFromSubverse(Context.User.Identity.Name, subverseName))
                {
                    // abort join
                    return null;
                }
            }            

            return Groups.Add(Context.ConnectionId, subverseName);
        }

        // remove a user from a subverse chat room
        public Task LeaveSubverseChatRoom(string subverseName)
        {
            return Groups.Remove(Context.ConnectionId, subverseName);
        }
    }

}
