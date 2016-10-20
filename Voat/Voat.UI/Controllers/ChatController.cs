using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Common;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
   
    public class ChatController : BaseController
    {
        private ChatRoom[] _availableRooms = new ChatRoom[] {
            new ChatRoom() { ID = "whatever", Name = "Whatever", Description = "Whatever - You know the deal" },
            new ChatRoom() { ID = "news", Name = "News", Description = "Discuss any news topics or submissions" },
            //new ChatRoom() { ID = "music", Name = "Music", Description = "You ever hear that one song?" },
            //new ChatRoom() { ID = "goatchat", Name = "Goat Chat", Description = "Random location for random goats" },
            //new ChatRoom() { ID = "dev", Name = "Development", Description = "if(you.WriteSoftware()) me.chatWith(you);" },
            };
        // GET: Chat
        public ActionResult Index(string subverseName = null)
        {
            ChatRoom defaultRoom = _availableRooms[0];
            ChatRoom selectedRoom = _availableRooms.FirstOrDefault(x => x.ID.Equals(subverseName, StringComparison.OrdinalIgnoreCase));

            if (selectedRoom == null)
            {
                selectedRoom = defaultRoom;
            }
            return View(new ChatViewModel() { Room = selectedRoom, AvailableRooms = _availableRooms});
        }
    }
}