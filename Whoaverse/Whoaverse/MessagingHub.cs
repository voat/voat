using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Voat
{
    [HubName("messagingHub")]
    public class MessagingHub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients
            if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(message))
            {
                if (name.Equals("atko", StringComparison.OrdinalIgnoreCase))
                {
                    name = "randomuser";
                }
                
                Clients.All.addNewMessageToPage(name, message);
            }
        }
    }
}