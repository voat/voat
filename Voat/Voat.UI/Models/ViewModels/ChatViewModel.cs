using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Models.ViewModels
{
   
    public class ChatViewModel
    {
        public ChatRoom Room { get; set; }
        //public string EnforceSubverseBans { get; set; }
        public IEnumerable<ChatRoom> AvailableRooms { get; set; }
    }
}