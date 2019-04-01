using BugTracker.Models;
using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class ProjectRepository
    {
        private ApplicationDbContext DBContext { get; set; }

        public ProjectRepository(ApplicationDbContext dBContext)
        {
            DBContext = dBContext;
        }

        public Project GetProject(string id) => DBContext.Projects.FirstOrDefault(project => project.Id.ToString() == id);

        public List<Project> GetUserProjects(string userId) => DBContext.Projects
            .Where(project => project.Users.Any(user => userId == user.Id))
            .ToList();

        public List<Project> GetUserProjects(ApplicationUser applicationUser) => DBContext.Projects
            .Where(project => project.Users.Any(user => applicationUser == user))
            .ToList();

        public List<Project> GetAllProjects() => DBContext.Projects.ToList();
    }
}