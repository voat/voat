using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Controllers;
using Voat.Models.ViewModels;

namespace Voat.UI.Controllers
{
    [Authorize]
    public class VoteController : BaseController
    {
        [HttpGet]
        public ActionResult Index(int id)
        {
            object model = null;
            return View("Index", model);
        }
        [HttpGet]
        public ActionResult Create(string subverse)
        {
            if (!VoatSettings.Instance.EnableVotes)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            var model = new Voat.Data.Models.Vote();
            model.VoteOptions = new List<Data.Models.VoteOption>();
            model.VoteOptions.Add(new Data.Models.VoteOption());
            model.VoteOptions.Add(new Data.Models.VoteOption());

            return View("Edit", model);
        }
        [HttpPost]
        public ActionResult Save([FromBody] Voat.Data.Models.Vote vote)
        {
            if (ModelState.IsValid) {

            }
            return Create("astom");
        }
        [HttpGet]
        public ActionResult Edit(string subverse, int id)
        {
            object model = null;
            return View("Edit", model);
        }
        [HttpGet]
        public ActionResult Delete(string subverse, int id)
        {
            object model = null;
            return View("Delete", model);
        }
        [HttpGet]
        public ActionResult Element(string type)
        {
            var typeName = type.TrimSafe();
            if (!String.IsNullOrEmpty(typeName))
            {
                switch (typeName.ToLower())
                {
                    case "voteoption":
                        return PartialView("_Option", new Voat.Data.Models.VoteOption());
                        break;
                    case "contributioncountrestriction":
                        return PartialView("_EditVoteRestriction", new Voat.Voting.Restrictions.ContributionCountRestriction());
                        break;
                    case "contributionpointrestriction":
                        return PartialView("_EditVoteRestriction", new Voat.Voting.Restrictions.ContributionPointRestriction());
                        break;
                    case "contributionvoterestriction":
                        return PartialView("_EditVoteRestriction", new Voat.Voting.Restrictions.ContributionVoteRestriction());
                        break;
                    case "memberagerestriction":
                        return PartialView("_EditVoteRestriction", new Voat.Voting.Restrictions.MemberAgeRestriction());
                        break;
                    case "subscriberrestriction":
                        return PartialView("_EditVoteRestriction", new Voat.Voting.Restrictions.SubscriberRestriction());
                        break;
                    case "moderatoroutcome":
                        return PartialView("_EditVoteOutcome", new Voat.Voting.Outcomes.RemoveModeratorOutcome());
                        break;

                }
            }
            return new EmptyResult();
        }

    }
}
