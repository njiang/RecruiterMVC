using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TestHireChannelMVC.Models
{
    public class Job
    {
        public Guid JobID { get; set; }
        public string Title { get; set; }
        public string Employer { get; set; }
        public DateTime PostDate { get; set; }
        public string Description { get; set; }
        public Guid UserID { get; set; }
    }

    public class JobContext : DbContext
    {
        public DbSet<Job> Users { get; set; }
    }
}