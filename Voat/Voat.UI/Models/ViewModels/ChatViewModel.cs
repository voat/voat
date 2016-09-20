using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Models.ViewModels
{
    public class ChatRoom {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

    }
    public class ChatViewModel
    {
        public ChatRoom Room { get; set; }
        //public string EnforceSubverseBans { get; set; }
        public ChatRoom[] AvailableRooms { get; set; }
    }
}