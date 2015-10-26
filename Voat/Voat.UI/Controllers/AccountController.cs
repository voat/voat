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

namespace Voat.Controllers
{
    [Authorize]
    public class AccountController : AsyncController
    {
        public AccountController()
            : this(new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
        {
            var provider = new DpapiDataProtectionProvider("VoatUI");
            UserManager.UserValidator = new UserValidator<VoatUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await UserManager.FindAsync(model.UserName, model.Password);
            var tmpuser = await UserManager.FindByNameAsync(model.UserName);

            // invalid credentials, increment failed login attempt and lockout account
            if (user == null)
            {
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

            // if account is locked out, and a correct password has been given, notify the user of lockout and don't allow login
            if (await UserManager.IsLockedOutAsync(tmpuser.Id))
            {
                ModelState.AddModelError("", "This account has been locked out for security reasons. Try again later.");
                return View(model);
            }

            // when token is verified correctly, clear the access failed count used for lockout
            await UserManager.ResetAccessFailedCountAsync(user.Id);

            // get user IP address
            string clientIpAddress = UserHelper.UserIpAddress(Request);

            // save last login ip and timestamp
            user.LastLoginFromIp = clientIpAddress;
            user.LastLoginDateTime = DateTime.Now;
            await UserManager.UpdateAsync(user);

            // sign in and continue
            await SignInAsync(user, model.RememberMe);
            // read User Theme preference and set value to cookie 
            UserHelper.SetUserStylePreferenceCookie(UserHelper.UserStylePreference(user.UserName));
            return RedirectToLocal(returnUrl);
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
        [ValidateAntiForgeryToken]
        [ValidateCaptcha]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {

            if (Settings.RegistrationDisabled)
            {
                return View("RegistrationDisabled");
            }

            if (!ModelState.IsValid) return View(model);

            if (!Business.Utilities.AccountSecurity.IsPasswordComplex(model.Password, model.UserName))
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
                    RegistrationDateTime = DateTime.Now,
                    LastLoginFromIp = clientIpAddress,
                    LastLoginDateTime = DateTime.Now
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

        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            ManageMessageId? message = result.Succeeded ? ManageMessageId.RemoveLoginSuccess : ManageMessageId.Error;
            return RedirectToAction("Manage", new { Message = message });
        }

        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.userid = User.Identity.Name;
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.WrongPassword ? "The password you entered does not match the one on our record."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.InvalidFileFormat ? "Please upload a .jpg or .png image."
                : message == ManageMessageId.UploadedFileToolarge ? "Uploaded file is too large. Current limit is 300 kb."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            ViewBag.userid = User.Identity.Name;
            return View();
        }

        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            var hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (hasPassword)
            {
                if (!ModelState.IsValid) return View(model);

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

                if (!ModelState.IsValid) return View(model);
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
            // If the user does not have an account, then prompt the user to create an account
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
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
                var user = new VoatUser { UserName = model.UserName };
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
            //Session["UserTheme"] = "light";
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
                return PartialView("_RemoveAccountPartial", linkedAccounts);
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
                return View("~/Views/Errors/Error.cshtml");
            }

            return RedirectToAction("Manage", new { message = ManageMessageId.WrongPassword });
        }

        // GET: /Account/UserPreferencesAbout
        [Authorize]
        public ActionResult UserPreferencesAbout()
        {
            try
            {
                using (var db = new voatEntities())
                {
                    var userPreferences = db.UserPreferences.Find(User.Identity.Name);

                    if (userPreferences != null)
                    {
                        // load existing preferences and return to view engine
                        var tmpModel = new UserAboutViewModel()
                        {
                            Bio = userPreferences.Bio,
                            Avatar = userPreferences.Avatar
                        };

                        return PartialView("_UserPreferencesAbout", tmpModel);
                    }
                    else
                    {
                        var tmpModel = new UserAboutViewModel();
                        return PartialView("_UserPreferencesAbout", tmpModel);
                    }
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserPreferencesAbout([Bind(Include = "Bio, Avatarfile")] UserAboutViewModel model)
        {
            // save changes
            using (var db = new voatEntities())
            {
                var userPreferences = db.UserPreferences.Find(User.Identity.Name);
                var tmpModel = new UserPreference();

                if (userPreferences == null)
                {
                    // create a new record for this user in userpreferences table
                    tmpModel.DisableCSS = false;
                    tmpModel.NightMode = false;
                    tmpModel.Language = "en";
                    tmpModel.OpenInNewWindow = false;
                    tmpModel.EnableAdultContent = false;
                    tmpModel.DisplayVotes = false;
                    tmpModel.DisplaySubscriptions = false;
                    tmpModel.UseSubscriptionsMenu = false;
                    tmpModel.UserName = User.Identity.Name;
                    tmpModel.Bio = model.Bio;
                }

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
                                        if (userPreferences == null)
                                        {
                                            tmpModel.Avatar = User.Identity.Name + ".jpg";
                                        }
                                        else
                                        {
                                            userPreferences.Avatar = User.Identity.Name + ".jpg";
                                        }
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

                if (userPreferences == null)
                {
                    db.UserPreferences.Add(tmpModel);
                    await db.SaveChangesAsync();
                }
                else
                {
                    userPreferences.Bio = model.Bio;
                    userPreferences.UserName = User.Identity.Name;
                    await db.SaveChangesAsync();
                }

            }

            return RedirectToAction("Manage");
        }

        // GET: /Account/UserPreferences
        [ChildActionOnly]
        public ActionResult UserPreferences()
        {
            try
            {
                using (var db = new voatEntities())
                {
                    var userPreferences = db.UserPreferences.Find(User.Identity.Name);

                    if (userPreferences != null)
                    {
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
                    else
                    {
                        var tmpModel = new UserPreferencesViewModel();
                        return PartialView("_UserPreferences", tmpModel);
                    }
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserPreferences([Bind(Include = "Disable_custom_css, Night_mode, OpenLinksInNewTab, Enable_adult_content, Public_subscriptions, Topmenu_from_subscriptions, Shortbio, Avatar")] UserPreferencesViewModel model)
        {
            if (!ModelState.IsValid) return View("Manage", model);

            // save changes
            string newTheme;
            using (var db = new voatEntities())
            {
                var userPreferences = db.UserPreferences.Find(User.Identity.Name);

                if (userPreferences != null)
                {
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
                else
                {
                    // create a new record for this user in userpreferences table
                    var tmpModel = new UserPreference
                    {
                        DisableCSS = model.Disable_custom_css ? true : false,
                        NightMode = model.Night_mode ? true : false,
                        Language = "en",
                        OpenInNewWindow = model.OpenLinksInNewTab ? true : false,
                        EnableAdultContent = model.Enable_adult_content ? true : false,
                        DisplayVotes = false,
                        DisplaySubscriptions = model.Public_subscriptions ? true : false,
                        UseSubscriptionsMenu = model.Topmenu_from_subscriptions,
                        UserName = User.Identity.Name
                    };
                    db.UserPreferences.Add(tmpModel);

                    await db.SaveChangesAsync();
                    newTheme = tmpModel.NightMode ? "dark" : "light";
                }
            }

            UserHelper.SetUserStylePreferenceCookie(newTheme);
            return RedirectToAction("Manage");
        }

        // POST: /Account/ToggleNightMode
        [Authorize]
        public async Task<ActionResult> ToggleNightMode()
        {
            string newTheme = "light";
            // save changes
            using (var db = new voatEntities())
            {
                var userPreferences = db.UserPreferences.Find(User.Identity.Name);

                if (userPreferences != null)
                {
                    // modify existing preferences
                    userPreferences.NightMode = !userPreferences.NightMode;
                    await db.SaveChangesAsync();
                    newTheme = userPreferences.NightMode ? "dark" : "light";
                    // apply theme change
                    //Session["UserTheme"] = UserHelper.UserStylePreference(User.Identity.Name);
                }
                else
                {
                    // create a new record for this user in userpreferences table
                    var tmpModel = new UserPreference
                    {
                        DisableCSS = false,
                        //Since if user has no pref, they must have been on the light theme
                        NightMode = true,
                        OpenInNewWindow = false,
                        EnableAdultContent = false,
                        DisplaySubscriptions = false,
                        UseSubscriptionsMenu = false,
                        UserName = User.Identity.Name
                    };
                    db.UserPreferences.Add(tmpModel);

                    await db.SaveChangesAsync();
                    // apply theme change
                    newTheme = "dark";
                    //Session["UserTheme"] = UserHelper.UserStylePreference(User.Identity.Name);
                }
            }

            UserHelper.SetUserStylePreferenceCookie(newTheme);
            Response.StatusCode = 200;
            return Json("Toggled Night Mode", JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/UserAccountEmail
        [Authorize]
        [ChildActionOnly]
        public ActionResult UserAccountEmail()
        {
            var existingEmail = UserManager.GetEmail(User.Identity.GetUserId());

            if (existingEmail == null) return PartialView("_ChangeAccountEmail");

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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccountEmail([Bind(Include = "EmailAddress")] UserEmailViewModel model)
        {
            if (!ModelState.IsValid) return View("Manage", model);

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
        [ValidateAntiForgeryToken]
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
                await UserManager.SendEmailAsync(user.Id, "Voat Password Reset Request", "You have requested to reset your Voat password. If you did not do this, please ignore this email. In order to open a page which will let you reset your Voat password, please click <a href=\"" + callbackUrl + "\">here</a>");
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
        [ValidateAntiForgeryToken]
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
        #endregion

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
            WrongPassword
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
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
        #endregion
    }
}