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

        // GET: Chat
        public ActionResult Index(string id = null, string access = null)
        {
            ChatRoom defaultRoom = ChatRoom.AvailableRooms[0];
            ChatRoom selectedRoom = ChatRoom.AvailableRooms.FirstOrDefault(x => x.ID.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (selectedRoom == null)
            {
                selectedRoom = defaultRoom;
            }

            bool authorized = selectedRoom.IsAccessAllowed(User.Identity.Name, access);
            if (!authorized)
            {
                return RedirectToAction("Password", new { id = selectedRoom.ID });
            }
            else
            {
                return View(new ChatViewModel() { Room = selectedRoom, AvailableRooms = ChatRoom.AvailableRooms.Where(x => !x.IsPrivate) });
            }

        }
        [HttpGet]
        [Authorize]
        public ActionResult Password(string id)
        {
            ChatRoom selectedRoom = ChatRoom.AvailableRooms.FirstOrDefault(x => x.ID.Equals(id, StringComparison.OrdinalIgnoreCase));
            return View("Password", new ChatViewModel() { Room = new ChatRoom() { ID = selectedRoom.ID } });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Password(ChatRoom room)
        {
            ChatRoom selectedRoom = ChatRoom.AvailableRooms.FirstOrDefault(x => x.ID.Equals(room.ID, StringComparison.OrdinalIgnoreCase));
            var accessHash = ChatRoom.GetAccessHash(User.Identity.Name, room.Passphrase);
            return RedirectToAction("Index", new { id = selectedRoom.ID, access = accessHash});
        }

        public ActionResult Create()
        {
            var id = Guid.NewGuid().ToString();

            var room = new ChatRoom() { ID = id, Name = id, Description = id, IsPrivate = true };
            ChatRoom.AvailableRooms.Add(room);

            return View(new ChatViewModel() { Room = room, AvailableRooms = ChatRoom.AvailableRooms.Where(x => !x.IsPrivate) });
        }
    }
}