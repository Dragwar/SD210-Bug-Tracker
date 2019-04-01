using System.Collections.Generic;
using BugTracker.Models;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public interface IUserRepository
    {
        ApplicationUser GetUser(string id);
        List<ApplicationUser> GetAllUsers();
    }
}