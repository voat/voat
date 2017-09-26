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
using System.Linq;
using System.Threading.Tasks;
using Voat.Models.ViewModels;
using System.Net;
using Voat.UI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Voat.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Voat.Configuration;
using Voat.Utilities;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Data;
using Voat.Caching;
using Microsoft.Extensions.Options;
using Voat.Common;
using Voat.Http.Filters;
using Voat.Http;
using Voat.IO.Email;

namespace Voat.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly VoatUserManager _userManager;
        private readonly SignInManager<VoatIdentityUser> _signInManager;
        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        //private readonly ILogger _logger;
        //private readonly string _externalCookieScheme;

        public AccountController(
            VoatUserManager userManager,
            SignInManager<VoatIdentityUser> signInManager
            //IOptions<IdentityCookieOptions> identityCookieOptions
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            //ILoggerFactory loggerFactory
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
            //_logger = loggerFactory.CreateLogger<AccountController>();

            UserManager = _userManager;
        }
       
        public VoatUserManager UserManager { get; private set; }

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
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                model.UserName = model.UserName.TrimSafe();
                model.Password = model.Password.TrimSafe();
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {

                        var cookie = HttpContext.Request.Cookies["theme"];
                        if (cookie != null && !String.IsNullOrEmpty(cookie))
                        {
                            Response.Cookies.Append("theme", "", new Microsoft.AspNetCore.Http.CookieOptions() { Expires = DateTime.UtcNow.AddDays(-1) });
                        }

                        return RedirectToLocal(returnUrl);
                    }
                    //if (result.RequiresTwoFactor)
                    //{
                    //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    //}
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "This account has been locked out for security reasons. Try again later.");
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);

            //CORE_PORT: Original code
            /*
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
                     await UserManager.AccessFailedAsync(tmpuser);

                     // if account is locked out, notify the user
                     if (await UserManager.IsLockedOutAsync(tmpuser))
                     {
                         ModelState.AddModelError("", "This account has been locked out for security reasons. Try again later.");
                         return View(model);
                     }
                 }

                 ModelState.AddModelError("", "Invalid username or password.");
                 return View(model);
             }
             else if (await UserManager.IsLockedOutAsync(user))
             {
                 ModelState.AddModelError("", "This account has been locked out for security reasons. Try again later.");
                 return View(model);
             }
             else
             {
                 //var userData = new UserData(user.UserName);
                 //userData.PreLoad();

                 // when token is verified correctly, clear the access failed count used for lockout
                 await UserManager.ResetAccessFailedCountAsync(user);

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
                 if(cookie != null && !String.IsNullOrEmpty(cookie))
                 {
                     //CORE_PORT: 
                     Response.Cookies.Append("theme", "", new Microsoft.AspNetCore.Http.CookieOptions() { Expires = DateTime.Now.AddDays(-1) });
                     //HttpContext.Response.Cookies["theme"].Expires = DateTime.Now.AddDays(-1);
                 }
                 return RedirectToLocal(returnUrl);
             } 
            */
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

            if (!VoatSettings.Instance.RegistrationEnabled)
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
            if (!VoatSettings.Instance.RegistrationEnabled)
            {
                return View("RegistrationDisabled");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (VoatSettings.Instance.ReservedUserNames.Contains(model.UserName.ToLower()))
            {
                ModelState.AddModelError(string.Empty, "The username entered is a reserved name.");
                return View(model);
            }
            var canBeRegistered = await UserHelper.CanUserNameBeRegistered(null, model.UserName);
            if (!canBeRegistered)
            {
                ModelState.AddModelError(string.Empty, "The username entered is too similar to an existing username. You must modify it in order to register an account.");
                return View(model);
            }

            if (!Utilities.AccountSecurity.IsPasswordComplex(model.Password, model.UserName, false))
            {
                ModelState.AddModelError(string.Empty, "Your password is not secure. You must use at least one uppercase letter, one lowercase letter, one number and one special character such as ?, ! or .");
                return View(model);
            }
           
            
            try
            {
                // get user IP address
                string clientIpAddress = Request.RemoteAddress();

                // check the number of accounts already in database with this IP address, if number is higher than max conf, refuse registration request
                var accountsWithSameIp = UserManager.Users.Count(x => x.LastLoginFromIp == clientIpAddress);
                if (accountsWithSameIp >= VoatSettings.Instance.MaxAllowedAccountsFromSingleIP)
                {
                    ModelState.AddModelError(string.Empty, "This device can not be used to create a voat account.");
                    return View(model);
                }

                var user = new VoatIdentityUser
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
                    await _signInManager.SignInAsync(user, isPersistent: false);

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
        public async Task<ActionResult> Manage(ManageMessageId? message)
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

            ViewBag.ReturnUrl = Url.Action("Manage");

            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                Description = "User Account",
                Name = User.Identity.Name,
                MenuType = MenuType.UserProfile,
                BasePath = null,
                Sort = null
            };
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserEmailViewModel = new UserEmailViewModel() { EmailAddress = user.Email };
            return View();
        }

        // POST: /Account/Manage
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewBag.UserName = User.Identity.Name;

            ViewBag.ReturnUrl = Url.Action("Manage");
            var user = await UserManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result, "OldPassword");
            return View("Manage", model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
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
        public ActionResult Delete()
        {
            return View();
        }

        // POST: /Account/DeleteAccount
        [Authorize]
        [HttpPost]
        //[PreventSpam(300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Domain.Models.DeleteAccountOptions model)
        {
            if (ModelState.IsValid)
            {
                var cmd = new Domain.Command.DeleteAccountCommand(model).SetUserContext(User);
                var response = await cmd.Execute();
                if (response.Success)
                {
                    await _signInManager.SignOutAsync();
                    return View("~/Views/Account/AccountDeleted.cshtml");
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                }
            }

            return View(model);
        } 

        //[Authorize]
        //public async Task<ActionResult> GetUserPreferencesAbout()
        //{
        //    var userPreferences = UserData.Preferences;
        //    var tmpModel = new UserAboutViewModel()
        //    {
        //        Bio = String.IsNullOrEmpty(userPreferences.Bio) ? STRINGS.DEFAULT_BIO : userPreferences.Bio,
        //        Avatar = userPreferences.Avatar
        //    };
        //    return PartialView("_UserPreferencesAbout", tmpModel);

        //    //return PartialView("_UserPreferencesAbout", tmpModel);
        //    //try
        //    //{
        //    //    using (var db = new VoatUIDataContextAccessor())
        //    //    {
        //    //        var userPreferences = GetUserPreference(db);

        //    //        var tmpModel = new UserAboutViewModel()
        //    //        {
        //    //            Bio = String.IsNullOrEmpty(userPreferences.Bio) ? STRINGS.DEFAULT_BIO : userPreferences.Bio,
        //    //            Avatar = userPreferences.Avatar
        //    //        };

        //    //        return PartialView("_UserPreferencesAbout", tmpModel);
        //    //    }
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return new EmptyResult();
        //    //}
        //}

        // POST: /Account/UserPreferences
        [Authorize]
        [HttpPost]
        [PreventSpam(15)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> UserPreferencesAbout([Bind("Bio, Avatarfile")] UserAboutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Manage", model);
            }

            ViewBag.UserName = User.Identity.Name;

            string avatarKey = null;

            //ThumbGenerator.GenerateThumbnail
            if (model.Avatarfile != null)
            {
                try
                {
                    var stream = model.Avatarfile.OpenReadStream();
                    var result = await ThumbGenerator.GenerateAvatar(stream, model.Avatarfile.FileName, model.Avatarfile.ContentType);
                    if (result.Success)
                    {
                        avatarKey = result.Response;
                    }
                    else
                    {
                        ModelState.AddModelError("Avatarfile", result.Message);
                        return View("Manage", model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Avatarfile", "Uploaded file is not recognized as a valid image.");
                    return RedirectToAction("Manage", new { Message = ManageMessageId.InvalidFileFormat });
                }
            }

            var bio = model.Bio.TrimSafe();
            
            //This is a hack
            var context = new VoatOutOfRepositoryDataContextAccessor();
            using (var repo = new Repository(User, context))
            {
                var p = await repo.GetUserPreferences(User.Identity.Name);
                if (bio != p.Bio)
                {
                    if (String.IsNullOrEmpty(bio))
                    {
                        p.Bio = "I tried to delete my bio but they gave me this instead";
                    }
                    else if (bio == STRINGS.DEFAULT_BIO)
                    {
                        p.Bio = null;
                    }
                    else
                    {
                        p.Bio = bio;
                    }
                }
                if (!String.IsNullOrEmpty(avatarKey))
                {
                    p.Avatar = avatarKey;   
                }
                await context.SaveChangesAsync();
            }

            /*
            using (var db = new VoatUIDataContextAccessor())
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
            */
            ClearUserCache();

            return RedirectToAction("Manage");
        }

        //// GET: /Account/UserPreferences
        //
        //public async Task<ActionResult> GetUserPreferences()
        //{

        //    var q = new QueryUserPreferences().SetUserContext(User);
        //    var model = await q.ExecuteAsync();
        //    return PartialView("_UserPreferences", model);

        //    //try
        //    //{
        //    //    using (var db = new VoatUIDataContextAccessor())
        //    //    {
        //    //        var userPreferences = GetUserPreference(db);

        //    //        // load existing preferences and return to view engine
        //    //        var tmpModel = new UserPreferencesViewModel
        //    //        {
        //    //            Disable_custom_css = userPreferences.DisableCSS,
        //    //            Night_mode = userPreferences.NightMode,
        //    //            OpenLinksInNewTab = userPreferences.OpenInNewWindow,
        //    //            Enable_adult_content = userPreferences.EnableAdultContent,
        //    //            Public_subscriptions = userPreferences.DisplaySubscriptions,
        //    //            Topmenu_from_subscriptions = userPreferences.UseSubscriptionsMenu
        //    //        };

        //    //        return PartialView("_UserPreferences", tmpModel);
        //    //    }
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return new EmptyResult();
        //    //}
        //}

        // POST: /Account/UserPreferences
        [Authorize]
        [HttpPost]
        [PreventSpam(15)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> UserPreferences(Domain.Models.UserPreferenceUpdate model)
        {
            ViewBag.UserName = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return View("Manage", model);
            }

            var cmd = new UpdateUserPreferencesCommand(model).SetUserContext(User);
            var result = await cmd.Execute();


            if (result.Success)
            {
                var newTheme = model.NightMode.Value ? "dark" : "light";
                UserHelper.SetUserStylePreferenceCookie(HttpContext, newTheme);
            }

            //// save changes
            //string newTheme;
            //using (var db = new VoatUIDataContextAccessor())
            //{
            //    var userPreferences = GetUserPreference(db);

            //    // modify existing preferences
            //    userPreferences.DisableCSS = model.Disable_custom_css;
            //    userPreferences.NightMode = model.Night_mode;
            //    userPreferences.OpenInNewWindow = model.OpenLinksInNewTab;
            //    userPreferences.EnableAdultContent = model.Enable_adult_content;
            //    userPreferences.DisplaySubscriptions = model.Public_subscriptions;
            //    userPreferences.UseSubscriptionsMenu = model.Topmenu_from_subscriptions;

            //    await db.SaveChangesAsync();
            //    newTheme = userPreferences.NightMode ? "dark" : "light";
            //}

            //ClearUserCache();
            //UserHelper.SetUserStylePreferenceCookie(newTheme);
            return RedirectToAction("Manage");
        }

        private UserPreference GetUserPreference(VoatOutOfRepositoryDataContextAccessor context)
        {
            var userPreferences = context.UserPreference.Find(User.Identity.Name);

            if (userPreferences == null)
            {
                userPreferences = new UserPreference();
                userPreferences.UserName = User.Identity.Name;
                Repository.SetDefaultUserPreferences(userPreferences);
                context.UserPreference.Add(userPreferences);
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
            var q = new QueryUserPreferences().SetUserContext(User);
            var preferences = await q.ExecuteAsync();

            var newPreferences = new Domain.Models.UserPreferenceUpdate();
            newPreferences.NightMode = !preferences.NightMode;

            var cmd = new UpdateUserPreferencesCommand(newPreferences).SetUserContext(User);
            var result = await cmd.Execute();

            string newTheme = newPreferences.NightMode.Value ? "dark" : "light";

            //// save changes
            //using (var db = new VoatUIDataContextAccessor())
            //{
            //    var userPreferences = GetUserPreference(db);

            //    userPreferences.NightMode = !userPreferences.NightMode;
            //    await db.SaveChangesAsync();

            //    newTheme = userPreferences.NightMode ? "dark" : "light";
            //}

            UserHelper.SetUserStylePreferenceCookie(HttpContext, newTheme);
            Response.StatusCode = 200;
            return Json("Toggled Night Mode" /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        }

        // GET: /Account/UserAccountEmail
        [Authorize]
        
        public async Task<ActionResult> GetUserAccountEmail()
        {
            //CORE_PORT: Changes in User Manager
            //TODO: This code needs to be unit tested
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var existingEmail = user.Email;

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
        [PreventSpam(15)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccountEmail([Bind("EmailAddress")] UserEmailViewModel model)
        {
            ViewBag.UserName = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return View("Manage", model);
            }

            if (!String.IsNullOrEmpty(model.EmailAddress))
            {
                // make sure no other accounts use this email address
                var existingAccount = await UserManager.FindByEmailAsync(model.EmailAddress);
                if (existingAccount != null)
                {
                    if (existingAccount.UserName == User.Identity.Name)
                    {
                        //we have the current user with the same email address, abort 
                        return View("Manage", model);
                    }
                    else
                    {
                        ViewBag.StatusMessage = "This email address is already in use.";
                        return View("Manage", model);
                    }
                }
            }

            //find current user
            var currentUser = await UserManager.FindByNameAsync(User.Identity.Name);

            // save changes
            var result = await UserManager.SetEmailAsync(currentUser, model.EmailAddress);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            AddErrors(result);
            return View("Manage", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [PreventSpam(5)]
        public async Task<JsonResult> CheckUsernameAvailability()
        {
            //CORE_PORT: Ported correctly?
            //var userNameToCheck = Request.Params["userName"];
            var userNameToCheck = Request.Form["userName"].FirstOrDefault();

            if (userNameToCheck == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("A username parameter is required for this function." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            }

            // check username availability
            var userNameAvailable = await UserManager.FindByNameAsync(userNameToCheck);

            if (userNameAvailable == null)
            {
                Response.StatusCode = 200;
                var response = new UsernameAvailabilityResponse
                {
                    Available = true
                };

                return Json(response /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Response.StatusCode = 200;
                var response = new UsernameAvailabilityResponse
                {
                    Available = false
                };
                return Json(response /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
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
                string code = await UserManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.GetUrl().Scheme);

                var response = await EmailSender.Instance.SendEmail(
                    user.Email,
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
            var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);
            
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

        //private IAuthenticationManager AuthenticationManager
        //{
        //    get
        //    {
        //        //CORE_PORT: Port
        //        throw new NotImplementedException("Core port not implemented");
        //        //return HttpContext.GetOwinContext().Authentication;
        //    }
        //}

        //private async Task SignInAsync(VoatIdentityUser user, bool isPersistent)
        //{
        //    //CORE_PORT: Port
        //    throw new NotImplementedException("Core port not implemented");
        //    //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //    //var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
        //    //AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        //}

        private void AddErrors(IdentityResult result, string key = "")
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(key, error.Description);
            }
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
            if (String.IsNullOrEmpty(returnUrl) && !String.IsNullOrEmpty(Request.Query["ReturnUrl"]))
            {
                returnUrl = Request.Query["ReturnUrl"];
            }
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        //private class ChallengeResult : HttpUnauthorizedResult
        //{
        //    public ChallengeResult(string provider, string redirectUri)
        //        : this(provider, redirectUri, null)
        //    {
        //    }

        //    public ChallengeResult(string provider, string redirectUri, string userId)
        //    {
        //        LoginProvider = provider;
        //        RedirectUri = redirectUri;
        //        UserId = userId;
        //    }

        //    public string LoginProvider { get; set; }

        //    public string RedirectUri { get; set; }

        //    public string UserId { get; set; }

        //    //CORE_PORT: Not ported
        //    //public override void ExecuteResult(ControllerContext context)
        //    //{
        //    //    var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
        //    //    if (UserId != null)
        //    //    {
        //    //        properties.Dictionary[XsrfKey] = UserId;
        //    //    }
        //    //    context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        //    //}
        //}

        #endregion Helpers
    }
}
