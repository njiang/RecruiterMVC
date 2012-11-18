using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections;

namespace TestHireChannelMVC.Controllers
{

    public class BoardData
    {
        public string value { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public string rowid { get; set; }  // a row with id is a position already existed in DB
        public string cellid { get; set; } // a cell id indicates the client ID or the JobID
    }

    public class Recruiter1Controller : Controller
    {
        //
        // GET: /Recruiter1/

        public ActionResult Index(string username)
        {
            //ViewBag.Message = "Welcome to ASP.NET MVC!";
            //String username = TempData["Username"] as String;
            Models.User user = UserHelper.Instance.GetUser(username);
            Models.RecruiterJobSeeker.RecruiterModel rjsmodel = new Models.RecruiterJobSeeker.RecruiterModel(user);
            TempData["UserID"] = user.UserID.ToString();
            TempData["Username"] = username;
            return View(rjsmodel);
        }


        public ActionResult PostJob(string username)
        {
            //String username = TempData["Username"] as String;
            Models.User user = UserHelper.Instance.GetUser(username);
            Models.Job job = new Models.Job();
            Models.RecruiterJobSeeker.JobUserSkillModel jsm = new Models.RecruiterJobSeeker.JobUserSkillModel(user, job);
            TempData["Model"] = jsm;
            TempData["Username"] = username;
            return View(jsm);
        }

        [HttpPost]
        public ActionResult PostNewJob()
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            Models.Job j = new Models.Job();
            j.JobID = Guid.NewGuid();
            j.Employer = Request.Form["Employer"];
            j.Description = Request.Form["Description"];
            j.Title = Request.Form["Title"];
            j.UserID = new Guid(Request.Form["UserID"]);
            j.PostDate = DateTime.Now;
            j.Status = 1;
            dataContext.Jobs.InsertOnSubmit(j);
            dataContext.SubmitChanges();
            for (int i = 1; i <= Int32.Parse(Request.Form["skillIndex"]); i++)
            {
                string skill = Request.Form["Skill" + i.ToString()];
                string year = Request.Form["Year" + i.ToString()];
                if (skill != String.Empty && year != string.Empty)
                {
                    Models.JobSkill jobskill = new Models.JobSkill();
                    jobskill.JobSkillID = Guid.NewGuid();
                    jobskill.JobID = j.JobID;
                    jobskill.Skill = skill;
                    jobskill.Year = (short)Int32.Parse(year);
                    dataContext.JobSkills.InsertOnSubmit(jobskill);
                }
            }
            dataContext.SubmitChanges();
            TempData["Username"] = Request.Form["username"];
            return RedirectToAction("Index", "Recruiter1", new { username = Request.Form["username"] });
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ListPostedJobs(string username)
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbuser = from u in dataContext.Users
                         where u.Login == username
                         select u;
            Models.User user = dbuser.First();
            Models.RecruiterJobSeeker.RecruiterModel jsmodel = new Models.RecruiterJobSeeker.RecruiterModel(user);
            TempData["UserID"] = user.UserID.ToString();
            return PartialView("_PostedJobs", jsmodel);
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
            return UserHelper.Instance.RenderPartialViewToString(this.ControllerContext, "_RecruiterSearchResult", this.ViewData, this.TempData, searchResult);
        }

        [HttpGet]
        public string SearchUserForJob(Guid recruiterID, Guid jobID)
        {
            Models.RecruiterJobSeeker.SearchResultModel searchResult = UserHelper.Instance.PerformSearchForJob(recruiterID, jobID);
            return UserHelper.Instance.RenderPartialViewToString(this.ControllerContext, "_RecruiterSearchResult", this.ViewData, this.TempData, searchResult);
        }

        [HttpGet]
        public ActionResult PinUser(Guid recruiterID, Guid userID, Guid jobID)
        {
            var dataContext = new Models.LinqModelHelperDataContext();
            Models.RecruiterPinnedUser rpu = new Models.RecruiterPinnedUser();
            rpu.PinID = Guid.NewGuid();
            rpu.JobID = jobID;
            rpu.UserID = userID;
            rpu.RecruiterID = recruiterID;
            rpu.PinnedTime = DateTime.UtcNow;
            dataContext.RecruiterPinnedUsers.InsertOnSubmit(rpu);
            dataContext.SubmitChanges();

            var dbusers = from u in dataContext.Users
                          where u.UserID == recruiterID
                          select u;
            Models.User user = dbusers.First();
            return RedirectToAction("Index", "Recruiter1", new { username = user.Login });
        }

        [HttpPost]
        public string EditCell(string id, string value)
        {
            return value;
        }


        [HttpPost]
        public void SaveBoard(string boardID, List<BoardData> board)
        {
            if (board != null)
            {
                var dataContext = new Models.LinqModelHelperDataContext();
                var dbboardOwnerID = from b in dataContext.RecruiterBoards
                                     where b.BoardID.ToString() == boardID
                                     select b;
                string boardOwnerID = dbboardOwnerID.First().OwnerID.ToString();

                // First set all the applied jobs with the columns to null
                //var dbappliedjobs = from aj in dataContext.UserAppliedJobs
                //                    join bc in dataContext.BoardColumns on aj.Stage equals bc.BoardID
                //                    select aj;

                //foreach (Models.UserAppliedJob aj in dbappliedjobs)
                //{
                //    aj.Stage = Guid.Empty;
                //}

                // Then delete all columns of the particular board
                var bcolumns = dataContext.BoardColumns.Where(c => c.BoardID.ToString() == boardID);
                dataContext.BoardColumns.DeleteAllOnSubmit(bcolumns);

                ArrayList columnIDs = new ArrayList();
                var boardcolumns = from r in board
                                   where r.row == 0
                                   orderby r.col
                                   select r;
                // Save columns
                foreach (BoardData bdata in boardcolumns)
                {
                    Models.BoardColumn boardColumn = new Models.BoardColumn();
                    boardColumn.BoardID = new Guid(boardID);
                    boardColumn.ColumnIndex = bdata.col;
                    boardColumn.ID = Guid.NewGuid();
                    boardColumn.Name = bdata.value;
                    dataContext.BoardColumns.InsertOnSubmit(boardColumn);
                    columnIDs.Add(boardColumn.BoardID);
                }
                
                // Now remove all values of a board
                ArrayList clientIDs = new ArrayList();
                ArrayList jobIDs = new ArrayList();
                int rowindex = -1;
                var boardelements = from r in board
                                    where r.row > 0
                                    select r;
                boardelements.OrderBy(r => new { r.col }).GroupBy(r => new { r.row });
                bool isRowUpdate = true;
                Guid currClientID = Guid.Empty;
                Guid currJobID = Guid.Empty;
                Guid currRowID = Guid.Empty;
                Models.User newclient = null;
                bool waitForJob = false;
                foreach (BoardData cell in boardelements)
                {
                    if (cell.row != rowindex)
                    {
                        // The first column of the row contains the rowid
                        isRowUpdate = String.IsNullOrEmpty(cell.rowid) ? false : true;
                        // Save the row
                        if (isRowUpdate)
                        {
                            // Find the row
                            var dbrow = from row in dataContext.JobBoardRows
                                        where row.RowID.ToString() == cell.rowid
                                        select row;
                            Models.JobBoardRow boardrow = dbrow.First();
                            boardrow.RowIndex = cell.row;
                            currRowID = boardrow.RowID;

                            waitForJob = false;
                        }
                        // TODO: what if the user adds a new row?
                        // TODO: we might want to ask the user to select from a client/position
                        // TODO: user should not be allowed to type arbitrarily in the new row
                        else
                        {
                            // TODO search for a user
                            newclient = new Models.User();
                            newclient.UserID = Guid.NewGuid();
                            newclient.FirstName = newclient.LastName = newclient.Login = cell.value;
                            dataContext.Users.InsertOnSubmit(newclient);

                            // Next cell should be the position info
                            waitForJob = true;
                        }
                        rowindex = cell.row;
                    }
                    
                    else if (waitForJob)
                    {
                        Models.Job newjob = new Models.Job();
                        newjob.JobID = Guid.NewGuid();
                        newjob.ClientID = newclient.UserID;
                        newjob.Title = cell.value;
                        newjob.PostDate = DateTime.UtcNow;
                        newjob.UserID = new Guid(boardOwnerID);
                        dataContext.Jobs.InsertOnSubmit(newjob);

                        Models.JobBoardRow newrow = new Models.JobBoardRow();
                        newrow.RowID = Guid.NewGuid();
                        newrow.RowIndex = cell.row;
                        newrow.ClientID = newclient.UserID;
                        newrow.BoardID = new Guid(boardID);
                        newrow.JobID = newjob.JobID;
                        dataContext.JobBoardRows.InsertOnSubmit(newrow);
                        currRowID = newrow.RowID;
                        currJobID = newjob.JobID;

                        Models.BoardContent bc = new Models.BoardContent();
                        bc.RowID = currRowID;
                        bc.ColumnIndex = 0;
                        bc.CellID = Guid.NewGuid();
                        bc.Value = newclient.FirstName;
                        dataContext.BoardContents.InsertOnSubmit(bc);

                        Models.BoardContent bc1 = new Models.BoardContent();
                        bc1.RowID = currRowID;
                        bc1.ColumnIndex = 1;
                        bc1.CellID = Guid.NewGuid();
                        bc1.Value = newjob.Title;
                        dataContext.BoardContents.InsertOnSubmit(bc1);
                        waitForJob = false;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(cell.value))
                            cell.value = "";
                        else
                        {
                            cell.value = cell.value.Replace("\n", "");
                            cell.value = cell.value.Trim();
                        }
                        if (!String.IsNullOrEmpty(cell.cellid))
                        {
                            // Existing cell, update it
                            string[] fields = cell.cellid.Split('_');
                            if (fields != null)
                            {
                                var dbcell = from c in dataContext.BoardContents
                                                where c.CellID.ToString() == cell.cellid
                                                select c;
                                Models.BoardContent bc = dbcell.First();
                                bc.ColumnIndex = cell.col;
                                bc.RowID = currRowID;
                                bc.Value = cell.value;
                            }


                            /*if (fields[0].Equals("Client", StringComparison.OrdinalIgnoreCase))
                            {
                                // The ID of this cell indicates the client ID
                                if (isUpdate)
                                {
                                    var client = from u in dataContext.Users
                                                    where u.UserID.ToString() == fields[2]
                                                    select u;
                                    Models.User user = client.First();
                                    user.FirstName = cell.value;
                                    user.LastName = cell.value;
                                    currClientID = user.UserID;
                                }
                                else
                                {
                                    Models.User user = new Models.User();
                                    user.UserID = Guid.NewGuid();
                                    user.FirstName = user.LastName = cell.value;
                                    dataContext.Users.InsertOnSubmit(user);
                                    currClientID = user.UserID;
                                }
                            }
                            else if (fields[0].Equals("Job", StringComparison.OrdinalIgnoreCase))
                            {
                                // The ID of this cell indicates the Job ID
                                if (isUpdate)
                                {
                                    var dbposition = from j in dataContext.Jobs
                                                        where j.JobID.ToString() == fields[2]
                                                        select j;
                                    Models.Job job = dbposition.First();
                                    job.Title = cell.value;
                                    currJobID = job.JobID;
                                }
                                else
                                {
                                    Models.Job job = new Models.Job();
                                    job.JobID = Guid.NewGuid();
                                    currJobID = job.JobID;
                                    job.Title = cell.value;
                                    job.PostDate = DateTime.UtcNow;
                                    job.UserID = new Guid(boardOwnerID);
                                    dataContext.Jobs.InsertOnSubmit(job);
                                }
                            } */
                        }
                        else
                        {
                            Models.BoardContent bc = new Models.BoardContent();
                            bc.RowID = currRowID;
                            bc.CellID = Guid.NewGuid();
                            bc.ColumnIndex = cell.col;
                            bc.Value = cell.value;
                            dataContext.BoardContents.InsertOnSubmit(bc);
                        }
                        
                            //else
                            //{
                            //    // These cells are applicants
                            //    Guid columnID = (Guid)columnIDs[cell.col];
                            //    if (isUpdate)
                            //    {
                            //        var dbapj = from j in dataContext.UserAppliedJobs
                            //                where j.JobID == currJobID
                            //                select j;

                            //    }
                            //    else
                            //    {
                            //        Models.UserAppliedJob uaj = new Models.UserAppliedJob();
                            //    }
                            //}
                    }
                    
                }
                dataContext.SubmitChanges();
            }
        }

    }
}


