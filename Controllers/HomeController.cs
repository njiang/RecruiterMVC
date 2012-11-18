using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestHireChannelMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            return View();
        }

        public ActionResult IndexRecruiter()
        {
            String username = TempData["Username"] as String; 
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var dataContext = new Models.LinqModelHelperDataContext(); 
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.RecruiterModel rjsmodel = new Models.RecruiterJobSeeker.RecruiterModel(user);
            TempData["UserID"] = user.UserID.ToString();
            return View(rjsmodel);
        }

        public ActionResult IndexJobSeeker()
        {
            String username = TempData["Username"] as String;
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.JobSeekerModel jsmodel = new Models.RecruiterJobSeeker.JobSeekerModel(user);
            TempData["UserID"] = user.UserID.ToString();
            return View(jsmodel);
        }


        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetUpdates(string username)
        {
            if (!String.IsNullOrEmpty(username))
            {
                var dataContext = new Models.LinqModelHelperDataContext();
                var dbuser = from u in dataContext.Users
                             where u.Login == username
                             select u;
                Models.User user = dbuser.First();
                if (user.UserType == 1)
                {

                }
                else if (user.UserType == 2)
                {
                    var dbPinnedUsers = from rpu in dataContext.RecruiterPinnedUsers
                                        where rpu.UserID == user.UserID && (!rpu.LastScanTime.HasValue || ((DateTime)rpu.LastScanTime) < DateTime.UtcNow)
                                        select rpu;
                    if (dbPinnedUsers.Count() > 0)
                    {
                        Models.RecruiterPinnedUser pinneduser = dbPinnedUsers.First();
                        var jsonRet = new { HasUpdates = true, UserType = 2, JobID = pinneduser.JobID};
                        pinneduser.LastScanTime = DateTime.UtcNow;
                        dataContext.SubmitChanges();
                        return Json(jsonRet, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            var jsonRet1 = new { HasUpdates = false };
            return Json(jsonRet1, JsonRequestBehavior.AllowGet);
        }
    }
}
