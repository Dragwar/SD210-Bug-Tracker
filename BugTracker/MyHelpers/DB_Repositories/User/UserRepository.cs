using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class UserRepository : IUserRepository
    {
        private ApplicationDbContext DBContext { get; set; }

        public UserRepository(ApplicationDbContext dBContext)
        {
            DBContext = dBContext;
        }

        public ApplicationUser GetUser(string id) => DBContext.Users.FirstOrDefault(user => user.Id == id);

        public List<ApplicationUser> GetAllUsers() => DBContext.Users.ToList();
    }
}