using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.RulesEngine;

namespace Voat.Rules.Chat
{

    [RuleDiscovery("Approves action if message isn't spammed", "approved = (IsSpam(message) == false)")]
    public class ChatMessageSpamRule : VoatRule
    {
        //thresholds
        public int _count = 3;
        public TimeSpan _timeSpanWindow = TimeSpan.FromSeconds(25);

        public ChatMessageSpamRule() : base("Chat Spam", "10.0", RuleScope.PostChatMessage, 1)
        {
            
        }
        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            ChatMessage message = context.PropertyBag.ChatMessage;
            var currentDate = Data.Repository.CurrentDate;

            if (message == null)
            {
                return CreateOutcome(RuleResult.Denied, "Rule needs chat message contexarstt");
            }
            
            var history = ChatHistory.History(message.RoomName);
            var historyArray = history.ToArray();

            //Copy Pasta
            if (historyArray.Any(x => x.UserName == message.UserName && x.Message.IsEqual(message.Message.TrimSafe())))
            {
                return CreateOutcome(RuleResult.Denied, "Chat message considered copy/paste spam");
            }

            ////Spammer
            var countInWindow = historyArray.Count(x => x.UserName == message.UserName && currentDate.Subtract(x.CreationDate) <= _timeSpanWindow);
            if (countInWindow >= _count)
            {
                return CreateOutcome(RuleResult.Denied, "Chat message considered spamming by user");
            }

            return base.EvaluateRule(context);
        }
    }
}
