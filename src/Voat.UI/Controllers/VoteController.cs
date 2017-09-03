using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Configuration;
using Voat.Controllers;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;
using Voat.Voting.Outcomes;
using Voat.Voting.Restrictions;

namespace Voat.UI.Controllers
{
    [Authorize]
    public class VoteController : BaseController
    {
        private static MemoryCacheHandler cache = new MemoryCacheHandler(false);

        [HttpGet]
        public ActionResult Index(int id)
        {
            object model = null;
            return View("Index", model);
        }

        [HttpGet]
        public async Task<ActionResult> View(int id)
        {
            var q = new QueryVote(id);
            var vote = await q.ExecuteAsync();
            return View(vote);
        }
        [HttpGet]
        public ActionResult Create(string subverse)
        {
            if (!VoatSettings.Instance.EnableVotes)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            var model = cache.Retrieve<Vote>($"VoteCreate:{User.Identity.Name}");
            if (model == null)
            {
                model = new Vote();
                model.Subverse = subverse;
            }

            return View("Edit", model);
        }
        public bool TryValidateModel2<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = null;// expression.Model;
            //object model = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider).Model;
            return TryValidateModel(model, name);
        }
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Save([FromBody] CreateVote model)
        {
            ModelState.Clear();

            //Map to Domain Model
            var domainModel = model.Map();

            //Reinsert Cache
            cache.Replace($"VoteCreate:{User.Identity.Name}", domainModel);


            var valResult = Voat.Validation.ValidationHandler.Validate(domainModel);
            if (valResult != null)
            {
                valResult.ForEach(x => ModelState.AddModelError(x.MemberNames.FirstOrDefault(), x.ErrorMessage));
            }
            if (ModelState.IsValid)
            {
                //Save Vote
                var cmd = new PersistVoteCommand(domainModel).SetUserContext(User);
                var result = await cmd.Execute();
                if (result.Success)
                {
                    return View("View", result.Response);
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }

            return PartialView("_Edit", domainModel);
        }
        
        [HttpGet]
        public async Task<ActionResult> Edit(string subverse, int id)
        {
            var q = new QueryVote(id);
            var vote = await q.ExecuteAsync();
            return View(vote);
        }
        [HttpGet]
        public ActionResult Delete(string subverse, int id)
        {
            object model = null;
            return View("Delete", model);
        }
        [HttpGet]
        public ActionResult Element(string subverse, string type)
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
