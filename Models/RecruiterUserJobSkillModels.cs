using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace TestHireChannelMVC.Models.RecruiterJobSeeker
{
    public class SearchResultModel
    {
        public Models.User User { get; protected set; }
        public String SearchTerm { get; protected set; }
        public Models.Job Job { get; protected set; }
        public List<Models.Job> Jobs { get; set; }
        public List<Models.User> Users { get; set; }

        public SearchResultModel(Models.User user, String searchTerm)
        {
            this.User = user;
            this.SearchTerm = searchTerm;
            Jobs = new List<Job>();
        }

        public SearchResultModel(Models.Job job, String searchTerm)
        {
            this.Job = job;
            this.SearchTerm = searchTerm;
            Users = new List<User>();
        }

        public SearchResultModel(Guid recruiterID, Models.Job job, String searchTerm)
        {
            this.User = new User();
            this.User.UserID = recruiterID;
            this.Job = job;
            this.SearchTerm = searchTerm;
            Users = new List<User>();
        }
    }

    public class CommonModel
    {
        public Models.User User { get; set; }
        public SearchResultModel SearchResult { get; set; }
    }


    #region Recruiter page model
    public class JobUserSkillModel
    {
        public User User { get; protected set; }
        public Job Job { get; protected set; }
        public List<Models.JobSkill> Skills { get; set; }
        public List<Models.User> AppliedUsers { get; protected set; }
        public List<User> PinnedUsers { get; protected set; }
        public JobSkill AdditionalSkill { get; set; }
        public SearchResultModel SearchResult { get; private set; }

        public JobUserSkillModel(User u, Job j)
        {
            User = u;
            Job = j;
            Skills = new List<JobSkill>();
            AdditionalSkill = new JobSkill();
            AppliedUsers = new List<Models.User>();
            PinnedUsers = new List<User>();
            var dataContext = new Models.LinqModelHelperDataContext();

            var dbskills = from s in dataContext.JobSkills
                           where s.JobID == j.JobID
                           select s;
            
            foreach (var s in dbskills)
            {
                Skills.Add(s);
            }

            GetAppliedUsers(dataContext);
            //GetPinnedUsers(dataContext);
        }

        private void GetAppliedUsers(LinqModelHelperDataContext dataContext)
        {
            var dbusers = from u in dataContext.Users
                          join aj in dataContext.UserAppliedJobs on u.UserID equals aj.UserID
                          where aj.JobID == this.Job.JobID
                          select u;

            foreach (var u in dbusers)
            {
                AppliedUsers.Add(u);
            }
        }

        private void GetPinnedUsers(LinqModelHelperDataContext dataContext)
        {
            var dbusers = from u in dataContext.Users
                         join ru in dataContext.RecruiterPinnedUsers on u.UserID equals ru.UserID
                         where ru.JobID == this.Job.JobID
                         select u;

            foreach (var u in dbusers)
            {
                
                PinnedUsers.Add(u);
            }
        }
    }


    public class ClientUserGroupModel
    {
        public Models.User Client { get; protected set; }
        public Models.Job Job { get; protected set; }
        public Models.User Applicant { get; protected set; }
        public Models.BoardColumn Column { get; protected set; }
        public int RowIndex { get; protected set; }
        public int ColumnIndex { get; protected set; }
        public int Columns { get; protected set; }
        public bool[] ColumnIndexBitmap { get; protected set; }

        public ClientUserGroupModel(Models.RecruiterBoard board, int rowindex, int columns, Models.User client, Models.Job job, Models.User applicant)
        {
            this.Client = client;
            this.Job = job;
            this.Applicant = applicant;
            this.RowIndex = rowindex;
            this.Columns = columns;
            var dataContext = new Models.LinqModelHelperDataContext();
            var dbresult = from aj in dataContext.UserAppliedJobs
                           join col in dataContext.BoardColumns on aj.Stage equals col.ID
                           where aj.UserID == applicant.UserID && aj.JobID == job.JobID
                           select col;
            this.Column = (Models.BoardColumn)dbresult.First();

            this.ColumnIndexBitmap = new bool[columns - 2];  // ignore the client name and position name columns
            for (int i = 0; i < columns - 2; i++)
                ColumnIndexBitmap[i] = false;

            var dbcolumns = from col in dataContext.BoardColumns
                            where col.BoardID == board.BoardID
                            orderby col.ColumnIndex
                            select col;
            int cindex = 0;
            foreach (var col in dbcolumns)
            {
                if (this.Column.ID == col.ID)
                {
                    ColumnIndexBitmap[cindex] = true;
                    this.ColumnIndex = cindex;
                    break;
                }
                cindex++;
            }
            
        }

    }

    public class BoardColumn
    {
        public int ColumnIndex { get; protected set; }
        public string ID { get; protected set; }
        public string Content { get; protected set; }

        public BoardColumn(string cid, int col, string content)
        {
            this.ID = cid;
            this.ColumnIndex = col;
            this.Content = content;
        }
    }

    public class BoardRow
    {
        public int RowIndex { get; protected set; }
        public string ID { get; protected set; }
        public List<BoardColumn> ColumnContents { get; protected set; }
        public Models.Job Job { get; protected set; }
        public Models.User Client { get; protected set; }

        public BoardRow(int row, string rid, Job job, User client)
        {
            this.RowIndex = row;
            this.ID = rid;
            this.Job = job;
            this.Client = client;
            this.ColumnContents = new List<BoardColumn>();
        }

        public void AddContent(int col, string cid, string content)
        {
            BoardColumn bcc = new BoardColumn(cid, col, content);
            this.ColumnContents.Add(bcc);
        }
    }

    public class RecruiterModel : CommonModel
    {
        //public Models.User Recruiter { get; protected set; }
        public List<JobUserSkillModel> JobModel { get; protected set; }
        public RecruiterBoard Board { get; protected set; }
        public List<string> Columns { get; protected set; }
        public List<BoardRow> BoardContents { get; protected set; }
        
        public List<ClientUserGroupModel> ClientUserGroup { get; protected set; }
        public List<List<User>> StagedUsers { get; protected set; }

        public RecruiterModel(User u)
        {
            this.User = u;
            JobModel = new List<JobUserSkillModel>();
            SearchResult = new SearchResultModel(u, string.Empty);
            this.ClientUserGroup = new List<ClientUserGroupModel>();
            StagedUsers = new List<List<Models.User>>();

            var dataContext = new Models.LinqModelHelperDataContext();
            Board = u.RecruiterBoards[0]; // each user has only one board?
            //var dbresult = from c in dataContext.Users
            //               join j in dataContext.Jobs on c.UserID equals j.ClientID
            //               join aj in dataContext.UserAppliedJobs on j.JobID equals aj.JobID
            //               join c1 in dataContext.Users on aj.UserID equals c1.UserID
            //               where j.UserID == u.UserID
            //               select new {c, j, c1};

            //dbresult.GroupBy( r => new { r.c.UserID, r.j.JobID} );



            Columns = new List<string>();
            //Columns.Add("Client");
            //Columns.Add("Position");
            //Columns.Add("Submitted");
            var dbcolumns = from col in dataContext.BoardColumns
                            where col.BoardID == Board.BoardID
                            orderby col.ColumnIndex
                            select col;

            int cindex = 0;
            foreach (var column in dbcolumns)
            {
                Columns.Add(column.Name);
                cindex++;
                //StagedUsers[column.ColumnIndex] = new List<Models.User>();

                //// Retrieve all the applicants at this stage for this job
                //var dbusers = from u1 in dataContext.Users
                //              join u2 in dataContext.UserAppliedJobs on u1.UserID equals u2.UserID
                //              where u2.Stage == column.ID
                //              select u1;
                //StagedUsers[column.ColumnIndex].AddRange(dbusers);
            }

            // select all the rows and cells of a Job Board
            var dbboardcontents = from row in dataContext.JobBoardRows
                                  join cell in dataContext.BoardContents on row.RowID equals cell.RowID into grow
                                  orderby row.RowIndex
                                  where row.BoardID == Board.BoardID
                                  select new { row = grow };

            this.BoardContents = new List<BoardRow>();
            BoardRow rowcontents = null;
            foreach (var result in dbboardcontents)
            {
                //ClientUserGroupModel cug = new ClientUserGroupModel(this.Board, i++, cindex, result.c, result.j, result.c1);
                //this.ClientUserGroup.Add(cug);
                var orderedcolumns = result.row.OrderBy(c => c.ColumnIndex);
                foreach (var cell in orderedcolumns)
                {
                    if (rowcontents == null)
                        rowcontents = new BoardRow(cell.JobBoardRow.RowIndex, cell.JobBoardRow.RowID.ToString(), cell.JobBoardRow.Job, cell.JobBoardRow.User);
                    if (String.IsNullOrEmpty(cell.Value))
                        rowcontents.AddContent(cell.ColumnIndex, cell.CellID.ToString(), "");
                    else
                        rowcontents.AddContent(cell.ColumnIndex, cell.CellID.ToString(), cell.Value);
                }
                this.BoardContents.Add(rowcontents);
                rowcontents = null;
            }

            //foreach (Job j in u.Jobs)
            //{
            //    JobUserSkillModel jsm = new JobUserSkillModel(u, j);
            //    JobModel.Add(jsm);
            //}
        }
    }

    public class RecruiterContext
    {

    }

    #endregion

    #region Job seeker model
    public class JobSeekerModel : CommonModel
    {
        //public User JobSeeker { get; protected set; }
        public List<RecruiterJobSeeker.JobUserSkillModel> AppliedJobs { get; protected set; }
        public List<RecruiterJobSeeker.JobUserSkillModel> PinnedJobs { get; protected set; }
        public List<User> FollowingRecruiters { get; protected set; }

        public JobSeekerModel(User u)
        {
            if (u != null)
            {
                this.User = u;
                this.SearchResult = new SearchResultModel(u, string.Empty);
                this.AppliedJobs = new List<JobUserSkillModel>();
                this.PinnedJobs = new List<JobUserSkillModel>();
                this.FollowingRecruiters = new List<Models.User>();
                var dataContext = new Models.LinqModelHelperDataContext();

                GetAppliedJobs(dataContext);
                GetPinnedJobs(dataContext);
                GetFollowingRecruiters(dataContext);
            }
        }

        private void GetAppliedJobs(LinqModelHelperDataContext dataContext)
        {
            var dbjobs = from j in dataContext.Jobs
                         join aj in dataContext.UserAppliedJobs on j.JobID equals aj.JobID
                         //join u in dataContext.Users on aj.UserID equals u.UserID
                         where aj.UserID == this.User.UserID
                         select j;

            foreach (var j in dbjobs)
            {
                JobUserSkillModel jusm = new JobUserSkillModel(this.User, j);
                AppliedJobs.Add(jusm);
            }
        }

        private void GetPinnedJobs(LinqModelHelperDataContext dataContext)
        {
            var dbjobs = from pj in dataContext.UserPinnedJobs
                         join j in dataContext.Jobs on pj.JobID equals j.JobID
                         where pj.UserID == this.User.UserID
                         select j;

            foreach (var j in dbjobs)
            {
                JobUserSkillModel jusm = new JobUserSkillModel(this.User, j);
                PinnedJobs.Add(jusm);
            }
        }

        private void GetFollowingRecruiters(LinqModelHelperDataContext dataContext)
        {
            var dbusers = from r in dataContext.FollowRecruiters
                         join u in dataContext.Users on r.RecruiterID equals u.UserID
                         where r.UserID == this.User.UserID
                         select u;

            foreach (var u in dbusers)
            {
                this.FollowingRecruiters.Add(u);
            }
        }
    }

    #endregion
}
