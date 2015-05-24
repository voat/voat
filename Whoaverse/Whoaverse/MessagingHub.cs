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
using Voat.Utils;

namespace Voat
{
    [HubName("messagingHub")]
    public class MessagingHub : Hub
    {
        // send a chat message to all users in a subverse room
        [Authorize]
        public void SendChatMessage(string name, string message, string subverseName)
        {
            if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(message) && !String.IsNullOrEmpty(subverseName))
            {
                var htmlEncodedMessage = WebUtility.HtmlEncode(message);
                Clients.Group(subverseName).appendChatMessage(Context.User.Identity.Name, htmlEncodedMessage);
            }
        }

        // add a user to a subverse chat room
        public Task JoinSubverseChatRoom(string subverseName)
        {
            return Groups.Add(Context.ConnectionId, subverseName);
        }

        // remove a user from a subverse chat room
        public Task LeaveSubverseChatRoom(string subverseName)
        {
            return Groups.Remove(Context.ConnectionId, subverseName);
        }
    }
}