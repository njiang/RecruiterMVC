using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TestHireChannelMVC.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime LastLogin { get; set; }
        public string LoginName { get; set; }
        public int UserType { get; set; }

    }
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}