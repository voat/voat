using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat
{
    //State should not be static here but this is a prototype 
    public class ChatRoom
    {
        public static List<ChatRoom> AvailableRooms = new List<ChatRoom>() {
            new ChatRoom() { ID = "whatever", Name = "Whatever", Description = "Whatever - You know the deal" },
            new ChatRoom() { ID = "news", Name = "News", Description = "Discuss any news topics or submissions" },
            new ChatRoom() { ID = "secret", Name = "Secret", Description = "Top Secret Chat, No one knows but us", IsPrivate = true },
            new ChatRoom() { ID = "passphrase", Name = "PassPhrase", Description = "This chat requires a secret key that no one knows ;)", IsPrivate = false, Passphrase = "password" },
            new ChatRoom() { ID = "hack", Name = "Hack This Room!", Description = "If you can hack this room, you get a badge.", IsPrivate = true, Passphrase = "{71EB6FB2-2EA9-4444-97AB-79E813BD8B58}" },
            new ChatRoom() { ID = "puttputt", Name = "Putt Putt's secret hideout", Description = "No one can hack this chat, no one!", IsPrivate = true, Passphrase = "puttputtfunfun" },
            };

        public static ChatRoom Find(string id)
        {
            return AvailableRooms.FirstOrDefault(x => x.ID == id);
        }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsPrivate { get; set; }

        public string Passphrase { get; set; }

        public string CreatedBy { get; set; }

        public bool IsAccessAllowed(string userName, string accessHash)
        {
            var allowed = true;
            if (IsPrivate || !String.IsNullOrEmpty(Passphrase))
            {
                if (!String.IsNullOrEmpty(userName))
                {
                    if (!String.IsNullOrEmpty(Passphrase))
                    {
                        return accessHash == GetAccessHash(userName, Passphrase);    
                    }
                    else
                    {
                        return true;
                    }
                }
                allowed = false;
            }
            return allowed;
        }
        public static string GetAccessHash(string userName, string passphrase)
        {
            string hash = "";
            if (!String.IsNullOrEmpty(userName))
            {
                var hashAlg = System.Security.Cryptography.MD5.Create();
                hash = System.Convert.ToBase64String(hashAlg.ComputeHash(System.Text.ASCIIEncoding.Unicode.GetBytes(String.Format("{0}-{1}", userName, passphrase))));
            }
            return hash;
        }
    }
}
