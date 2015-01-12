using System;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;

namespace Voat
{
    public class MessagingHub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
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