using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestHireChannelMVC.Controllers
{
    public class JobSeekerController : Controller
    {
        //
        // GET: /JobSeeker/
        
        public ActionResult Index(string username)
        {
            //String username = TempData["Username"] as String;
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.JobSeekerModel jsmodel = new Models.RecruiterJobSeeker.JobSeekerModel(user);
            return View(jsmodel);
        }

        [HttpPost]
        public string SearchForm(string searchterm)
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            string username = Request.Form["username"];
            var dbuser = from u in dataContext.Users
                        where u.Login == username
                        select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.SearchResultModel searchResult = UserHelper.Instance.PerformSearch(user, Request.Form["searchterm"]);
            return UserHelper.Instance.RenderPartialViewToString(this.ControllerContext, "_JobSeekerSearchResult", this.ViewData, this.TempData, searchResult);
        }

        [HttpGet]
        [OutputCache(NoStore=true, Duration = 0, VaryByParam = "*")]
        public ActionResult ListAppliedJobs(string username)
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.JobSeekerModel jsmodel = new Models.RecruiterJobSeeker.JobSeekerModel(user);
            TempData["UserID"] = user.UserID.ToString();
            return PartialView("_AppliedJobs", jsmodel);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ListPinnedJobs(string username)
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.JobSeekerModel jsmodel = new Models.RecruiterJobSeeker.JobSeekerModel(user);
            TempData["UserID"] = user.UserID.ToString();
            return PartialView("_PinnedJobs", jsmodel);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ListFollowingRecruiters(string username)
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.JobSeekerModel jsmodel = new Models.RecruiterJobSeeker.JobSeekerModel(user);
            TempData["UserID"] = user.UserID.ToString();
            return PartialView("_FollowingRecruiters", jsmodel);
        }

        [HttpGet]
        public void ApplyJob(Guid jobID, Guid userID) //string username)
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            //var dbuser = from u in dataContext.Users
            //             where u.Login == username
            //             select u;
            //Models.User user = (Models.User)dbuser.First();
            Models.UserAppliedJob uaj = new Models.UserAppliedJob();
            uaj.UserID = userID; // user.UserID;
            uaj.JobID = jobID;
            uaj.ID = Guid.NewGuid();
            uaj.AppliedTime = DateTime.UtcNow;

            // Find the "Submitted" column GUID
            var coldb = from c in dataContext.BoardColumns
                        where c.Name == "Submitted"
                        select c.ID;

            uaj.Stage = coldb.First();
            dataContext.UserAppliedJobs.InsertOnSubmit(uaj);
            dataContext.SubmitChanges();
        }

        [HttpGet]
        public void RetrieveChanges(string username)
        {
            
        }
    }
}
