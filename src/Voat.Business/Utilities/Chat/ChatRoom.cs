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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat
{
    public class UserContext
    {
        public UserContext()
        {

        }
        public UserContext(string userName)
        {
            DisplayName = userName;
            UserName = UserName;
            IsAnonymized = false;
        }

        public string DisplayName { get; set; }

        public bool IsAnonymized { get; set; }

        public string UserName { get; set; }

    }

    //State should not be static here but this is a prototype 
    public class ChatRoom
    {
        private static string[] aliasNamePrefixes = { "Slippy", "Slappy", "Simons", "Samsonite", "Swanson", "Swammy", "Swenson" }; 

        public static List<ChatRoom> AvailableRooms = new List<ChatRoom>() {
            new ChatRoom() { ID = "whatever", Name = "Whatever", Description = "Whatever - You know the deal" },
            new ChatRoom() { ID = "news", Name = "News", Description = "Discuss any news topics or submissions" },
            new ChatRoom() { ID = "secret", Name = "Secret", Description = "Top Secret Chat, No one knows but us", IsPrivate = true },
            new ChatRoom() { ID = "passphrase", Name = "PassPhrase", Description = "This chat requires a secret key that no one knows ;)", IsPrivate = false, Passphrase = "password" },
            new ChatRoom() { ID = "hack", Name = "Hack This Room!", Description = "If you can hack this room, you get a badge.", IsPrivate = true, Passphrase = "{71EB6FB2-2EA9-4444-97AB-79E813BD8B58}" },
            new ChatRoom() { ID = "puttputt", Name = "Putt Putt's secret hideout", Description = "No one can hack this chat, no one!", IsPrivate = true, Passphrase = "puttputtfunfun" },
            new ChatRoom() { ID = "anon", Name = "Anon (Proto)", Description = "Anon Room (WARNING: This Chat May Have Bugs Exposing You!)", IsAnonymized = true },
        };

        public static ChatRoom Find(string id)
        {
            return AvailableRooms.FirstOrDefault(x => x.ID == id);
        }

        public List<UserContext> Users { get; set; } = new List<UserContext>();

        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsAnonymized { get; set; }

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

        public ChatMessage CreateMessage(string userName, string formattedMessage)
        {
            //find user
            var user = Users.FirstOrDefault(x => x.UserName == userName);

            if (user == null)
            {
                user = AddUser(userName);
            }


            var chatMessage = new ChatMessage()
            {
                RoomID = ID,
                User = user,
                Message = formattedMessage,
                CreationDate = Data.Repository.CurrentDate,
            };
            return chatMessage;
        }

        public UserContext AddUser(string userName)
        {
            var user = new UserContext() { UserName = userName, DisplayName = IsAnonymized ? GenerateAlias() : userName, IsAnonymized = IsAnonymized };
            Users.Add(user);
            return user;
        }
        private string GenerateAlias()
        {
            var available = false;
            var random = new Random();
            var alias = "";

            while (!available)
            {
                var prefix = aliasNamePrefixes[random.Next(0, aliasNamePrefixes.Length - 1)];
                var number = random.Next(0, 99);
                alias = String.Format("{0}-{1}", prefix, number.ToString().PadLeft(2, '0'));
                available = !this.Users.Any(x => x.DisplayName == alias); 
            }

            return alias;
        }
    }
}
