/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Models.ViewModels;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Voat.App_Start;
using Voat.Data.Models;
using Voat.Utilities;
using Voat.Configuration;
using Voat.UI.Utilities;
using Voat.Data;
using Voat.Caching;
using Voat.Domain;

namespace Voat.Controllers
{
    [Authorize]
    public class AccountController : AsyncController
    {
        public AccountController()
            : this(new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
        {
            var provider = Startup.DataProtectionProvider;
            UserManager.UserValidator = new UserValidator<VoatUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            //Email issues: http://stackoverflow.com/questions/23455579/generating-reset-password-token-does-not-work-in-azure-website
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<VoatUser>(provider.Create("VoatTokenProvider"));
        }

        public AccountController(UserManager<VoatUser> userManager)
        {
            UserManager = userManager;

            // Configure user lockout defaults
            UserManager.UserLockoutEnabledByDefault = true;
            UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;
            UserManager.EmailService = new IdentityConfig.EmailService();
        }

        public UserManager<VoatUser> UserManager { get; private set; }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                // deny access to registered users
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindAsync(model.UserName, model.Password);

            // invalid credentials, increment failed login attempt and lockout account
            if (user == null)
            {
                var tmpuser = await UserManager.FindByNameAsync(model.UserName);
                // correct username was entered with wrong credentials
                if (tmpuser != null)
                {
                    // record failed login attempt and lockout account if failed login limit is reached
                    await UserManager.AccessFailedAsync(tmpuser.Id);

                    // if account is locked out, notify the user
                    if (await UserManager.IsLockedOutAsync(tmpuser.Id))
                    {
                        ModelState.AddModelError("", "This account has been locked out for security reasons. Try again later.");
                        return View(model);
                    }
                }

                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else if (await UserManager.IsLockedOutAsync(user.Id))
            {
                ModelState.AddModelError("", "This account has been locked out for security reasons. Try again later.");
                return View(model);
            }
            else
            {
                //var userData = new UserData(user.UserName);
                //userData.PreLoad();

                // when token is verified correctly, clear the access failed count used for lockout
                await UserManager.ResetAccessFailedCountAsync(user.Id);

                // get user IP address
                string clientIpAddress = UserHelper.UserIpAddress(Request);

                // save last login ip and timestamp
                user.LastLoginFromIp = clientIpAddress;
                user.LastLoginDateTime = Repository.CurrentDate;
                await UserManager.UpdateAsync(user);

                // sign in and continue
                await SignInAsync(user, model.RememberMe);

                // remove the theme cookie, it will be set to the user preference after the page reloads
                var cookie = HttpContext.Request.Cookies["theme"];
                if(cookie != null && !String.IsNullOrEmpty(cookie.Value))
                {
                    HttpContext.Response.Cookies["theme"].Expires = DateTime.Now.AddDays(-1);
                }
                return RedirectToLocal(returnUrl);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                // deny access to registered users
                return RedirectToAction("Index", "Home");
            }

            if (Settings.RegistrationDisabled)
            {
                return View("RegistrationDisabled");
            }

            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [VoatValidateAntiForgeryToken]
        [ValidateCaptcha]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (Settings.RegistrationDisabled)
            {
                return View("RegistrationDisabled");
            }

            if (!ModelState.IsValid)
                return View(model);

            if (!Utilities.AccountSecurity.IsPasswordComplex(model.Password, model.UserName, false))
            {
                ModelState.AddModelError(string.Empty, "Your password is not secure. You must use at least one uppercase letter, one lowercase letter, one number and one special character such as ?, ! or .");
                return View(model);
            }

            try
            {
                // get user IP address
                string clientIpAddress = UserHelper.UserIpAddress(Request);

                // check the number of accounts already in database with this IP address, if number is higher than max conf, refuse registration request
                var accountsWithSameIp = UserManager.Users.Count(x => x.LastLoginFromIp == clientIpAddress);
                if (accountsWithSameIp >= Settings.MaxAllowedAccountsFromSingleIP)
                {
                    ModelState.AddModelError(string.Empty, "This device can not be used to create a voat account.");
                    return View(model);
                }

                var user = new VoatUser
                {
                    UserName = model.UserName,
                    RegistrationDateTime = Repository.CurrentDate,
                    LastLoginFromIp = clientIpAddress,
                    LastLoginDateTime = Repository.CurrentDate
                };

                // try to create new user account
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);

                    // redirect new users to Welcome actionresult
                    return RedirectToAction("Welcome", "Home");
                }
                AddErrors(result);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened. You broke Voat.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.UserName = User.Identity.Name;
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.WrongPassword ? "The password you entered does not match the one on our record."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.InvalidFileFormat ? "Please upload a .jpg or .png image."
                : message == ManageMessageId.UploadedFileToolarge ? "Uploaded file is too large. Current limit is 300 kb."
                : message == ManageMessageId.UserNameMismatch ? "UserName entered does not match current account"
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        // POST: /Account/Manage
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            ViewBag.UserName = User.Identity.Name;

            var hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (hasPassword)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                var state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Manage", new { Message = ManageMessageId.WrongPassword });
        }

        // POST: /Account/LogOff
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
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
        [VoatValidateAntiForgeryToken]
        public ActionResult DeleteAccount(DeleteAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!User.Identity.Name.IsEqual(model.UserName))
                {
                    return RedirectToAction("Manage", new { message = ManageMessageId.UserNameMismatch });
                }
                else
                {
                    // require users to enter their password in order to execute account delete action
                    var user = UserManager.Find(User.Identity.Name, model.CurrentPassword);

                    if (user != null)
                    {
                        // execute delete action
                        if (UserHelper.DeleteUser(User.Identity.Name))
                        {
                            // delete email address and set password to something random
                            UserManager.SetEmail(User.Identity.GetUserId(), null);

                            string randomPassword = "";
                            using (SHA512 shaM = new SHA512Managed())
                            {
                                randomPassword = Convert.ToBase64String(shaM.ComputeHash(Encoding.UTF8.GetBytes(Path.GetRandomFileName())));
                            }

                            UserManager.ChangePassword(User.Identity.GetUserId(), model.CurrentPassword, randomPassword);

                            AuthenticationManager.SignOut();
                            return View("~/Views/Account/AccountDeleted.cshtml");
                        }

                        // something went wrong when deleting user account
                        return View("~/Views/Error/Error.cshtml");
                    }
                }
            }
            return RedirectToAction("Manage", new { message = ManageMessageId.WrongPassword });
        }

        // GET: /Account/UserPreferencesAbout
        [Authorize]
        public ActionResult GetUserPreferencesAbout()
        {
            try
            {
                using (var db = new voatEntities())
                {
                    var userPreferences = GetUserPreference(db);

                    var tmpModel = new UserAboutViewModel()
                    {
                        Bio = String.IsNullOrEmpty(userPreferences.Bio) ? STRINGS.DEFAULT_BIO : userPreferences.Bio,
                        Avatar = userPreferences.Avatar
                    };

                    return PartialView("_UserPreferencesAbout", tmpModel);
                }
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        // POST: /Account/UserPreferences
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 15, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> UserPreferencesAbout([Bind(Include = "Bio, Avatarfile")] UserAboutViewModel model)
        {
            ViewBag.UserName = User.Identity.Name;
            // save changes
            using (var db = new voatEntities())
            {
                var userPreferences = GetUserPreference(db);

                if (model.Avatarfile != null && model.Avatarfile.ContentLength > 0)
                {
                    // check uploaded file size is < 300000 bytes (300 kilobytes)
                    if (model.Avatarfile.ContentLength < 300000)
                    {
                        try
                        {
                            using (var img = Image.FromStream(model.Avatarfile.InputStream))
                            {
                                if (img.RawFormat.Equals(ImageFormat.Jpeg) || img.RawFormat.Equals(ImageFormat.Png))
                                {
                                    // resize uploaded file
                                    var thumbnailResult = await ThumbGenerator.GenerateAvatar(img, User.Identity.Name, model.Avatarfile.ContentType);
                                    if (thumbnailResult)
                                    {
                                        userPreferences.Avatar = User.Identity.Name + ".jpg";
                                    }
                                    else
                                    {
                                        // unable to generate thumbnail
                                        ModelState.AddModelError("", "Uploaded file is not recognized as a valid image.");
                                        return RedirectToAction("Manage", new { Message = ManageMessageId.InvalidFileFormat });
                                    }
                                }
                                else
                                {
                                    // uploaded file was invalid
                                    ModelState.AddModelError("", "Uploaded file is not recognized as an image.");
                                    return RedirectToAction("Manage", new { Message = ManageMessageId.InvalidFileFormat });
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // uploaded file was invalid
                            ModelState.AddModelError("", "Uploaded file is not recognized as an image.");
                            return RedirectToAction("Manage", new { Message = ManageMessageId.InvalidFileFormat });
                        }
                    }
                    else
                    {
                        // refuse to save the file and explain why
                        ModelState.AddModelError("", "Uploaded image may not exceed 300 kb, please upload a smaller image.");
                        return RedirectToAction("Manage", new { Message = ManageMessageId.UploadedFileToolarge });
                    }
                }

                var bio = model.Bio.TrimSafe();

                if (String.IsNullOrEmpty(bio))
                {
                    userPreferences.Bio = "I tried to delete my bio but they gave me this instead";
                }
                else if (bio == STRINGS.DEFAULT_BIO)
                {
                    userPreferences.Bio = null;
                }
                else
                {
                    userPreferences.Bio = bio;
                }
                await db.SaveChangesAsync();
            }

            ClearUserCache();

            return RedirectToAction("Manage");
        }

        // GET: /Account/UserPreferences
        [ChildActionOnly]
        public ActionResult GetUserPreferences()
        {
            try
            {
                using (var db = new voatEntities())
                {
                    var userPreferences = GetUserPreference(db);

                    // load existing preferences and return to view engine
                    var tmpModel = new UserPreferencesViewModel
                    {
                        Disable_custom_css = userPreferences.DisableCSS,
                        Night_mode = userPreferences.NightMode,
                        OpenLinksInNewTab = userPreferences.OpenInNewWindow,
                        Enable_adult_content = userPreferences.EnableAdultContent,
                        Public_subscriptions = userPreferences.DisplaySubscriptions,
                        Topmenu_from_subscriptions = userPreferences.UseSubscriptionsMenu
                    };

                    return PartialView("_UserPreferences", tmpModel);
                }
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        // POST: /Account/UserPreferences
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 15, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> UserPreferences([Bind(Include = "Disable_custom_css, Night_mode, OpenLinksInNewTab, Enable_adult_content, Public_subscriptions, Topmenu_from_subscriptions, Shortbio, Avatar")] UserPreferencesViewModel model)
        {
            ViewBag.UserName = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return View("Manage", model);
            }

            // save changes
            string newTheme;
            using (var db = new voatEntities())
            {
                var userPreferences = GetUserPreference(db);

                // modify existing preferences
                userPreferences.DisableCSS = model.Disable_custom_css;
                userPreferences.NightMode = model.Night_mode;
                userPreferences.OpenInNewWindow = model.OpenLinksInNewTab;
                userPreferences.EnableAdultContent = model.Enable_adult_content;
                userPreferences.DisplaySubscriptions = model.Public_subscriptions;
                userPreferences.UseSubscriptionsMenu = model.Topmenu_from_subscriptions;

                await db.SaveChangesAsync();
                newTheme = userPreferences.NightMode ? "dark" : "light";
            }

            ClearUserCache();
            UserHelper.SetUserStylePreferenceCookie(newTheme);
            return RedirectToAction("Manage");
        }

        private UserPreference GetUserPreference(voatEntities context)
        {
            var userPreferences = context.UserPreferences.Find(User.Identity.Name);

            if (userPreferences == null)
            {
                userPreferences = new UserPreference();
                userPreferences.UserName = User.Identity.Name;
                Repository.SetDefaultUserPreferences(userPreferences);
                context.UserPreferences.Add(userPreferences);
            }

            return userPreferences;
        }

        private void ClearUserCache(string userName = null)
        {
            userName = String.IsNullOrEmpty(userName) ? User.Identity.Name : userName;

            CacheHandler.Instance.Remove(CachingKey.UserPreferences(userName));
            CacheHandler.Instance.Remove(CachingKey.UserInformation(userName));
        }

        // POST: /Account/ToggleNightMode
        [Authorize]
        public async Task<ActionResult> ToggleNightMode()
        {
            string newTheme = "light";

            // save changes
            using (var db = new voatEntities())
            {
                var userPreferences = GetUserPreference(db);

                userPreferences.NightMode = !userPreferences.NightMode;
                await db.SaveChangesAsync();

                newTheme = userPreferences.NightMode ? "dark" : "light";
            }

            UserHelper.SetUserStylePreferenceCookie(newTheme);
            Response.StatusCode = 200;
            return Json("Toggled Night Mode", JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/UserAccountEmail
        [Authorize]
        [ChildActionOnly]
        public ActionResult GetUserAccountEmail()
        {
            var existingEmail = UserManager.GetEmail(User.Identity.GetUserId());

            if (existingEmail == null)
            {
                return PartialView("_ChangeAccountEmail");
            }

            var userEmailViewModel = new UserEmailViewModel
            {
                EmailAddress = existingEmail
            };

            return PartialView("_ChangeAccountEmail", userEmailViewModel);
        }

        // POST: /Account/UserAccountEmail
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 15, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccountEmail([Bind(Include = "EmailAddress")] UserEmailViewModel model)
        {
            ViewBag.UserName = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return View("Manage", model);
            }

            // make sure no other accounts use this email address
            var existingAccount = await UserManager.FindByEmailAsync(model.EmailAddress);
            if (existingAccount != null)
            {
                ViewBag.StatusMessage = "This email address is already in use.";
                return View("Manage", model);
            }

            // save changes
            var result = await UserManager.SetEmailAsync(User.Identity.GetUserId(), model.EmailAddress);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            AddErrors(result);
            return View("Manage", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [PreventSpam(DelayRequest = 5, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public JsonResult CheckUsernameAvailability()
        {
            var userNameToCheck = Request.Params["userName"];
            if (userNameToCheck == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("A username parameter is required for this function.", JsonRequestBehavior.AllowGet);
            }

            // check username availability
            var userNameAvailable = UserManager.FindByName(userNameToCheck);

            if (userNameAvailable == null)
            {
                Response.StatusCode = 200;
                var response = new UsernameAvailabilityResponse
                {
                    Available = true
                };

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Response.StatusCode = 200;
                var response = new UsernameAvailabilityResponse
                {
                    Available = false
                };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        private class UsernameAvailabilityResponse
        {
            public bool Available { get; set; }
        }

        #region password reset

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [VoatValidateAntiForgeryToken]
        [ValidateCaptcha]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(
                    user.Id, 
                    "Voat Password Reset Request", 
                    $"You have requested to reset your Voat password.<br/><br/>If you did not do this, please ignore this email.<br/><br/>To reset your password please click the following link or copy and paste the url into your browser address bar: <a href=\"{callbackUrl}\">{callbackUrl}</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.SelectedSubverse = string.Empty;
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            ViewBag.SelectedSubverse = string.Empty;
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View();
        }

        #endregion password reset

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

        private async Task SignInAsync(VoatUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
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
            Error,
            InvalidFileFormat,
            UploadedFileToolarge,
            WrongPassword,
            UserNameMismatch
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && !String.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
            {
                returnUrl = Request.QueryString["ReturnUrl"];
            }
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
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
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion Helpers
    }
}
