using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Controllers;
using Voat.Domain.Models;
using Voat.Models.ViewModels;
using Voat.Voting.Outcomes;
using Voat.Voting.Restrictions;

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

            var model = new Voat.Domain.Models.Vote();
            model.Title = "";
            model.Content = "Main Vote Content";

            //model.Options = new List<Data.Models.VoteOption>();
            model.Options.Add(new Domain.Models.VoteOption() { Title = "Title 1", Content = "Content 1" });
            model.Options.Add(new Domain.Models.VoteOption() { Title = "Title 2", Content = "Content 2" });

            return View("Edit", model);
        }
        [HttpPost]
        public ActionResult Save([FromBody] Voat.Domain.Models.CreateVote model)
        {
            var domainModel = Map(model);
            var result = TryValidateModel(domainModel);

            if (!ModelState.IsValid)
            {
                return PartialView("_Edit", domainModel);
            }
            return Create("ast");
        }
        private Vote Map(CreateVote transform)
        {
            var model = new Vote();
            model.Title = transform.Title;
            model.Content = transform.Content;
            model.Subverse = transform.Subverse;

            foreach (var r in transform.Restrictions)
            {
                var o = r.Construct<VoteRestriction>();
                string value = r.Options.ToString();
                var options = r.Options;
                model.Restrictions.Add(o);
            }
            transform.Options.ForEach(x =>
            {
                var option = new VoteOption();
                option.Title = x.Title;
                option.Content = x.Content;

                x.Outcomes.ForEach(o =>
                {
                    var outcome = o.Construct<VoteOutcome>();
                    option.VoteOutcomes.Add(outcome);
                });
                model.Options.Add(option);
            });
            return model;
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

            if (typeName.IsEqual("VoteOption"))
            {
                return PartialView("_Option", new VoteOption());
            }

            var metadata = VoteMetadata.Instance.FindByName(typeName);

            if (metadata != null)
            {
                if (metadata.Type.IsSubclassOf(typeof(VoteRestriction)))
                {
                    return PartialView("_EditVoteRestriction", Activator.CreateInstance(metadata.Type));
                } 
                else if (metadata.Type.IsSubclassOf(typeof(VoteOutcome)))
                {
                    return PartialView("_EditVoteOutcome", Activator.CreateInstance(metadata.Type));
                }
            }
            return new EmptyResult();
        }

    }
}
