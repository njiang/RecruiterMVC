using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using TestHireChannelMVC.Models;

namespace TestHireChannelMVC.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        TempData["Username"] = model.UserName;
                        LinqModelHelperDataContext dbcontext = new LinqModelHelperDataContext();
                        var dbuser = from u in dbcontext.Users
                                       where u.Login == model.UserName
                                       select u;
                        User user = dbuser.First();
                        if (user.UserType == 1)
                            //return RedirectToAction("IndexRecruiter", "Home");
                            return RedirectToAction("Index", "Recruiter1", new { username = user.Login }); //, new RouteValueDictionary(model.UserName));
                        return RedirectToAction("Index", "JobSeeker", new { username = user.Login }); //, new RouteValueDictionary(model.UserName));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View(new RegisterModel());
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            //if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);

                    // Save user to DB
                    LinqModelHelperDataContext dbcontext = new LinqModelHelperDataContext();
                    Guid userID = Guid.NewGuid();
                    Models.User user = new Models.User();
                    user.UserID = userID;
                    user.FirstName = model.UserName;
                    user.LastName = model.UserName;
                    user.Login = model.UserName;
                    user.UserType = (short)model.UserType;
                    user.LastLogin = DateTime.Now;
                    user.Email = model.Email;
                    user.City = model.City;
                    user.State = model.State;
                    user.Country = model.Country;
                    user.Zip = model.Zip;
                    user.WillingToRelocate = model.WillingToRelocate;
                    dbcontext.Users.InsertOnSubmit(user);
                    for (int i = 1; i <= Int32.Parse(Request.Form["skillIndex"]); i++)
                    {
                        string skill = Request.Form["Skill" + i.ToString()];
                        string year = Request.Form["Year" + i.ToString()];
                        if (skill != String.Empty && year != string.Empty)
                        {
                            Models.UserSkill userskill = new Models.UserSkill();
                            userskill.UserSkillID = Guid.NewGuid();
                            userskill.UserID = user.UserID;
                            userskill.Skill = skill;
                            userskill.Year = (short)Int32.Parse(year);
                            dbcontext.UserSkills.InsertOnSubmit(userskill);
                        }
                    }
                    dbcontext.SubmitChanges();

                    TempData["Username"] = model.UserName; 
                    if (user.UserType == 1)
                        return RedirectToAction("Index", "Recruiter1", new { username = model.UserName });
                    return RedirectToAction("Index", "JobSeeker", new { username = model.UserName });
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
