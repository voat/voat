using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class CommentController : Controller
    {
        [Authorize]
        public JsonResult VoteComment(int commentId, int typeOfVote)
        {
            if (User.Identity.IsAuthenticated)
            {
                string loggedInUser = User.Identity.Name;

                if (typeOfVote == 1)
                {
                    //perform upvoting or resetting
                    VotingComments.UpvoteComment(commentId, loggedInUser);
                }
                else if (typeOfVote == -1)
                {
                    //perform downvoting or resetting
                    VotingComments.DownvoteComment(commentId, loggedInUser);
                }
                return Json("Voting ok", JsonRequestBehavior.AllowGet);
            }
            return Json("Voting unauthorized.", JsonRequestBehavior.AllowGet);
        }

    }
}