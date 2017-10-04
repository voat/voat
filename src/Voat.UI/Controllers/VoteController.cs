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
using Voat.Data;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;
using Voat.UI.Utils;
using Voat.Utilities;
using Voat.Voting.Outcomes;
using Voat.Voting.Restrictions;

namespace Voat.UI.Controllers
{
    public class VoteController : BaseController
    {
        private IViewRenderService _viewRenderService;
        public VoteController(IViewRenderService viewRenderService)
        {
            _viewRenderService = viewRenderService;
        }
        [HttpGet]
        public ActionResult Index(int id)
        {
            object model = null;
            return View("Index", model);
        }

        [HttpGet]
        public async Task<ActionResult> View(string subverse, int id)
        {
            var agg = await VoteAggregate.Load(id);
            //var q = new QueryVote(id);
            //var vote = await q.ExecuteAsync();

            if (agg.Vote == null)
            {
                return ErrorController.ErrorView(ErrorType.NotFound);
            }
            if (!agg.Vote.Subverse.IsEqual(subverse))
            {
                return ErrorController.ErrorView(ErrorType.NotFound);
            }
            return View("Index", agg);
        }

        private bool VotesEnabled
        {
            get
            {
                return VoatSettings.Instance.EnableVotes || User.IsInAnyRole(new[] { Voat.Domain.Models.UserRole.GlobalAdmin, Voat.Domain.Models.UserRole.Admin, Voat.Domain.Models.UserRole.DelegateAdmin });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Create(string subverse)
        {
            if (!VotesEnabled)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            var q = new QuerySubverse(subverse);
            var sub = await q.ExecuteAsync();

            if (sub == null)
            {
                return ErrorController.ErrorView(ErrorType.SubverseNotFound);
            }

            var model = new Vote();
            model.Subverse = sub.Name;
            
            return View("Edit", model);
        }
        public bool TryValidateModel2<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = null;// expression.Model;
            //object model = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider).Model;
            return TryValidateModel(model, name);
        }
        public class SaveVoteResponse
        {
            public int ID { get; set; }
            public string Html { get; set; }
        }
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Save([FromBody] CreateVote model)
        {
            //This needs fixed
            if (!VotesEnabled)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            try
            {

                ModelState.Clear();

                //Map to Domain Model
                var domainModel = model.Map();

                ////Reinsert Cache
                //cache.Replace($"VoteCreate:{User.Identity.Name}", domainModel);


                var valResult = Voat.Validation.ValidationHandler.Validate(domainModel);
                if (valResult != null)
                {
                    valResult.ForEach(x => ModelState.AddModelError(x.MemberNames.FirstOrDefault(), x.ErrorMessage));
                }
                CommandResponse<SaveVoteResponse> response = new CommandResponse<SaveVoteResponse>(new SaveVoteResponse(), Status.Success, "");

                if (ModelState.IsValid)
                {
                    //Save Vote
                    var cmd = new PersistVoteCommand(domainModel).SetUserContext(User);
                    var result = await cmd.Execute();
                    if (result.Success)
                    {
                        response.Response.ID = result.Response.ID;
                        response.Response.Html = await _viewRenderService.RenderToStringAsync("_View", await VoteAggregate.Load(result.Response));
                        return JsonResult(response);
                        //return PartialView("_View", result.Response);
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }
                }

                response.Status = Status.Error;
                response.Response.Html = await _viewRenderService.RenderToStringAsync("_Edit", domainModel, ModelState);
                return JsonResult(response);
                //return PartialView("_Edit", domainModel);

            }
            catch (Exception ex)
            {
                var x = ex;
                throw ex;
            }

            
        }
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Edit(string subverse, int id)
        {
            var q = new QueryVote(id);
            var vote = await q.ExecuteAsync();

            if (vote == null)
            {
                return ErrorController.ErrorView(ErrorType.NotFound);
            }

            if (!vote.CreatedBy.IsEqual(User.Identity.Name))
            {
                return ErrorController.ErrorView(ErrorType.Unauthorized);
            }

            return View(vote);
        }
        [HttpGet]
        [Authorize]
        public ActionResult Delete(string subverse, int id)
        {
            object model = null;
            return View("Delete", model);
        }
        [HttpGet]
        [Authorize]
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

        [HttpGet]
        public async Task<ActionResult> List(string subverse)
        {
            var q = new QueryVotes(subverse, SearchOptions.Default);

            var votes = await q.ExecuteAsync();

            var list = new ListViewModel<Vote>();
            list.Context = new DomainReference(DomainType.Subverse, subverse);
            list.Items = new PaginatedList<Vote>(votes, 0, 100);
            list.Title = $"{subverse} Votes";
            list.Description = $"Votes listed in v/{subverse}";
            
            return View("VoteList", list);
        }
    }
}
