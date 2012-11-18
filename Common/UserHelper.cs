using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace TestHireChannelMVC.Controllers
{
    public class UserHelper
    {
        private static UserHelper instance;

        private UserHelper() { }

        public static UserHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserHelper();
                }
                return instance;
            }
        }

        public Models.User GetUser(string username)
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            return user;
        }

        public Models.RecruiterJobSeeker.SearchResultModel PerformSearch(Models.User user, string keywords)
        {
            Models.RecruiterJobSeeker.SearchResultModel result = new Models.RecruiterJobSeeker.SearchResultModel(user, keywords);
            if (keywords != string.Empty)
            {
                if (user.UserType == 1)
                {
                    var dataContext = new Models.LinqModelHelperDataContext();
                    var dbusers = from u in dataContext.Users
                                 select u;

                    //result.Jobs.AddRange(dbjobs.Where(j => j.Description.ToUpper().Contains(keywords.ToUpper())));
                    foreach (Models.User u in dbusers)
                    {
                        //if (j.Description.ToUpper().Contains(keywords.ToUpper()) || j.Title.ToUpper().Contains(keywords.ToUpper()))
                        //{
                            result.Users.Add(u);
                        //}
                    }
                }
                else if (user.UserType == 2)
                {
                    var dataContext = new Models.LinqModelHelperDataContext();
                    var dbjobs = from j in dataContext.Jobs
                                 select j;

                    //result.Jobs.AddRange(dbjobs.Where(j => j.Description.ToUpper().Contains(keywords.ToUpper())));
                    foreach (Models.Job j in dbjobs)
                    {
                        if (j.Description.ToUpper().Contains(keywords.ToUpper()) || j.Title.ToUpper().Contains(keywords.ToUpper()))
                        {
                            result.Jobs.Add(j);
                        }
                    }
                }
            }
            return result;
        }

        public Models.RecruiterJobSeeker.SearchResultModel PerformSearchForJob(Guid recruiterID, Guid jobID)
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbjobs = from j in dataContext.Jobs
                         where j.JobID == jobID
                         select j;
            Models.Job job = dbjobs.First();
            Models.RecruiterJobSeeker.SearchResultModel result = new Models.RecruiterJobSeeker.SearchResultModel(recruiterID, job, String.Empty);
            
            var dbusers = from u in dataContext.Users
                          select u;

            //result.Jobs.AddRange(dbjobs.Where(j => j.Description.ToUpper().Contains(keywords.ToUpper())));
            foreach (Models.User u in dbusers)
            {
                //if (j.Description.ToUpper().Contains(keywords.ToUpper()) || j.Title.ToUpper().Contains(keywords.ToUpper()))
                //{
                result.Users.Add(u);
                //}
            }
            return result;
        }

        public string RenderPartialViewToString(ControllerContext controllerContext, string viewName, ViewDataDictionary viewData, TempDataDictionary tempData, object model)
        {
            viewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                if (viewResult.View != null)
                {
                    ViewContext viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);

                    viewResult.View.Render(viewContext, sw);
                    return sw.GetStringBuilder().ToString();
                }
            }
            return String.Empty;
        }

        public string RenderViewToString(ControllerContext controllerContext, string viewName, ViewDataDictionary viewData, TempDataDictionary tempData, object model)
        {
            viewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindView(controllerContext, "SearchResult", "_Layout");
                ViewContext viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);

                try
                {
                    viewResult.View.Render(viewContext, sw);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                }
                return sw.GetStringBuilder().ToString();
            }
        }


        public string RenderViewToString(ControllerContext controllerContext, string viewPath, string masterPath,  ViewDataDictionary viewData, TempDataDictionary tempData, object model)
        {
            Stream filter = null;
            ViewPage viewPage = new ViewPage();

            //Right, create our view
            viewData.Model = model;
            viewPage.ViewContext = new ViewContext(controllerContext, new WebFormView(controllerContext, viewPath, masterPath), viewData, tempData, viewPage.ViewContext.HttpContext.Response.Output);

            //Get the response context, flush it and get the response filter. 
            var response = viewPage.ViewContext.HttpContext.Response;
            response.Flush();
            var oldFilter = response.Filter;

            try
            {
                //Put a new filter into the response 
                filter = new MemoryStream();
                response.Filter = filter;

                //Now render the view into the memorystream and flush the response 
                viewPage.ViewContext.View.Render(viewPage.ViewContext, viewPage.ViewContext.HttpContext.Response.Output);
                response.Flush();

                //Now read the rendered view. 
                filter.Position = 0;
                var reader = new StreamReader(filter, response.ContentEncoding);
                return reader.ReadToEnd();
            }
            finally
            {
                //Clean up. 
                if (filter != null)
                {
                    filter.Dispose();
                }

                //Now replace the response filter 
                response.Filter = oldFilter;
            }
        } 

    }
}