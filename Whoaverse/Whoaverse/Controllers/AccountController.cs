﻿/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;

namespace Whoaverse.Controllers
{
    [Authorize]
    public class AccountController : AsyncController
    {

        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;

            // Configure user lockout defaults
            UserManager.UserLockoutEnabledByDefault = true;
            UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);

                if (user == null)
                {

                    // Check if correct username was entered with wrong password and increment failed attempts - lockout account
                    var tmpuser = await UserManager.FindByNameAsync(model.UserName);
                    if (tmpuser != null)
                    {
                        await UserManager.AccessFailedAsync(tmpuser.Id);

                        // Check if correct username was entered and see if account was locked out, notify
                        if (await UserManager.IsLockedOutAsync(tmpuser.Id))
                        {                            
                            ModelState.AddModelError("", "This account has been locked out for security reasons. Try again later.");
                            return View(model);
                        }  
                    }

                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }                              

                // When token is verified correctly, clear the access failed count used for lockout
                await UserManager.ResetAccessFailedCountAsync(user.Id);

                // Sign in and continue
                await SignInAsync(user, model.RememberMe);
                return RedirectToLocal(returnUrl);               

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // begin recaptcha helper setup
                var recaptchaHelper = this.GetRecaptchaVerificationHelper();

                if (String.IsNullOrEmpty(recaptchaHelper.Response))
                {
                    ModelState.AddModelError("", "Captcha answer cannot be empty");
                    return View(model);
                }

                var recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();

                if (recaptchaResult != RecaptchaVerificationResult.Success)
                {
                    ModelState.AddModelError("", "Incorrect captcha answer");
                    return View(model);
                }
                // end recaptcha helper setup

                try
                {
                    var user = new ApplicationUser() { UserName = model.UserName, RecoveryQuestion = model.RecoveryQuestion, Answer = model.Answer, Email = model.Email };

                    user.RegistrationDateTime = DateTime.Now;

                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        // redirect new users to Welcome actionresult
                        return RedirectToAction("Welcome", "Home");
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Something bad happened. You broke Whoaverse.");
                }                
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Email()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        // GET: /Account/CheckUsernameForPasswordRecovery
        [HttpGet]
        [AllowAnonymous]
        // 20 minutes delay.
        [PreventSpam(DelayRequest = 1200, ErrorMessage = "Sorry, you must wait 20 minutes before trying again.")]
        public ActionResult CheckUsernameForPasswordRecovery()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        // POST: /Account/CheckUsernameForPasswordRecovery
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        // 20 minutes delay
        //[PreventSpam(DelayRequest = 1200, ErrorMessage = "Sorry, you must wait 20 minutes before trying again.")]
        public async Task<ActionResult> CheckUsernameForPasswordRecovery(UsernameEntryForPasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                // begin recaptcha helper setup
                var recaptchaHelper = this.GetRecaptchaVerificationHelper();

                if (String.IsNullOrEmpty(recaptchaHelper.Response))
                {
                    ModelState.AddModelError("", "Captcha answer cannot be empty");
                    return View(model);
                }

                var recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();

                if (recaptchaResult != RecaptchaVerificationResult.Success)
                {
                    ModelState.AddModelError("", "Incorrect captcha answer");
                    return View(model);
                }
                // end recaptcha helper setup

                try
                {
                    var account = UserManager.FindByName(model.UserName);

                    if (account == null)
                    {
                        ModelState.AddModelError("", "Username you entered does not exists.");
                        return View(model);
                    }

                    if (string.IsNullOrWhiteSpace(account.Email))
                    {
                        ModelState.AddModelError("", "The account you entered does not have an email.");
                        return View();
                    }

                    if (string.IsNullOrWhiteSpace(account.RecoveryQuestion) || string.IsNullOrWhiteSpace(account.Answer))
                    {
                        ModelState.AddModelError("", "The account you entered does not have any recovery question or answer.");
                        return View(model);
                    }

                    return RedirectToAction("AnswerQuestionForPasswordRecovery", "Account", account.UserName);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Something bad happened. You broke Whoaverse.");
                }
            }
            return View(model);
        }

        // GET: AnswerQuestionForPasswordRecovery
        [HttpGet]
        [AllowAnonymous]
        // 20 minutes delay.
        [PreventSpam(DelayRequest = 1200, ErrorMessage = "Sorry, you must wait 20 minutes before trying again.")]
        public async Task<ActionResult> AnswerQuestionForPasswordRecovery(string username)
        {
            ViewBag.SelectedSubverse = string.Empty;
            if (username == null)
            {
                ModelState.AddModelError("", "A Username is required!");
                return RedirectToAction("CheckUsernameForPasswordRecovery", "Account");
            }
            try
            {
                var account = await UserManager.FindByNameAsync(username);

                if (account == null)
                {
                    ModelState.AddModelError("", "Username you entered does not exists.");
                    return View();
                }

                if (string.IsNullOrWhiteSpace(account.Email))
                {
                    ModelState.AddModelError("", "The account you entered does not have an email.");
                    return View();
                }

                if (string.IsNullOrWhiteSpace(account.RecoveryQuestion) || string.IsNullOrWhiteSpace(account.Answer))
                {
                    ModelState.AddModelError("", "The account you entered does not have any recovery question or answer.");
                    return View();
                }

                return View(new AnswerQuestionForPasswordRecoveryModel(account.RecoveryQuestion));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened. You broke Whoaverse.");
            }
            return View();
        }

        // POST: /Account/CheckUsernameForPasswordRecovery
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        // 20 minutes delay
        [PreventSpam(DelayRequest = 1200, ErrorMessage = "Sorry, you must wait 20 minutes before trying again.")]
        public async Task<ActionResult> AnswerQuestionForPasswordRecovery(AnswerQuestionForPasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                #region recaptcha helper setup
                var recaptchaHelper = this.GetRecaptchaVerificationHelper();

                if (String.IsNullOrEmpty(recaptchaHelper.Response))
                {
                    ModelState.AddModelError("", "Captcha answer cannot be empty");
                    return View(model);
                }

                var recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();

                if (recaptchaResult != RecaptchaVerificationResult.Success)
                {
                    ModelState.AddModelError("", "Incorrect captcha answer");
                    return View(model);
                }
                #endregion
                if (!RouteData.Values.ContainsKey("username"))
                {
                    ModelState.AddModelError("", "A username is required.");
                    return View(model);
                }
                try
                {
                    var account = await UserManager.FindByNameAsync(RouteData.Values["username"].ToString());

                    if (account == null)
                    {
                        ModelState.AddModelError("", "Username you entered does not exists.");
                        return View(model);
                    }

                    if (string.IsNullOrWhiteSpace(account.RecoveryQuestion) || string.IsNullOrWhiteSpace(account.Answer))
                    {
                        ModelState.AddModelError("", "The account you entered does not have any recovery question or answer.");
                        return View(model);
                    }

                    if (account.Answer != model.Answer)
                    {
                        ModelState.AddModelError("", "The answer you entered is incorrect.");
                        return View(model);
                    }
                    
                    // Generate a new Password Recovery Token
                    Random randomGenerator = new Random();
                    var guessNum = randomGenerator.Next();
                    account.PasswordRecoveryToken = guessNum;
                    await UserManager.UpdateAsync(account);

                    // Send an email for the final step in password recovery.
                    var link = string.Format("{0}/{1}/{2}/{3}",ConstantUtility.HostName, "Account/ReplaceForgottenPassword", account.UserName, guessNum);
                    EmailUtility.sendPasswordRecoveryMail(
                        new EmailUtility.PasswordRecoveryMessage(account.Email, "Whoaverse Password Recovery", link)
                    );
                    ViewBag.WaitForTokenLink = true;
                    return View(model);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Something bad happened. You broke Whoaverse.");
                }
            }
            return View(model);
        }

        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.userid = User.Identity.GetUserName();
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.ChangePasswordAndRecoveryInfoSuccess ? "Your password and recovery question and answer have been changed."
                : message == ManageMessageId.SetPasswordAndRecoveryInfoSuccess ? "Your password has been set and your recovery question and answer have been changed."
                : message == ManageMessageId.ChangeRecoveryInfoSuccess ? "Your recovery question and answer have been changed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            bool hasNewQuestion = !string.IsNullOrWhiteSpace(model.NewRecoveryQuestion);
            bool hasNewAnswer = !string.IsNullOrWhiteSpace(model.NewAnswer);
            bool hasChangedRecoveryInfo = false;
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (hasNewQuestion && hasNewAnswer)
            {
                var updateUser = UserManager.FindById(User.Identity.GetUserId());
                updateUser.RecoveryQuestion = model.NewRecoveryQuestion;
                updateUser.Answer = model.NewAnswer;
                IdentityResult result = await UserManager.UpdateAsync(updateUser);
                hasChangedRecoveryInfo = result.Succeeded;
            }

            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        if (hasChangedRecoveryInfo)
                            return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordAndRecoveryInfoSuccess});
                        else
                            return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        if (hasChangedRecoveryInfo)
                            return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordAndRecoveryInfoSuccess });
                        else
                            return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            if (hasChangedRecoveryInfo)
                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangeRecoveryInfoSuccess });

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            try
            {
                var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
                ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
                return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        // POST: /Account/DeleteAccount
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAccount(DeleteAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    AuthenticationManager.SignOut();

                    // execute delete action
                    if (Whoaverse.Utils.User.DeleteUser(User.Identity.Name))
                    {
                        // deletion executed without errors 
                        return View("~/Views/Account/AccountDeleted.cshtml");
                    }
                    else
                    {
                        return View("~/Views/Errors/Error.cshtml");
                    }
                }
                else
                {
                    return View("~/Views/Errors/Error.cshtml");
                }
            }
            else
            {
                return View("~/Views/Errors/Error.cshtml");
            }                     
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            ChangePasswordAndRecoveryInfoSuccess,
            SetPasswordAndRecoveryInfoSuccess,
            ChangeRecoveryInfoSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}