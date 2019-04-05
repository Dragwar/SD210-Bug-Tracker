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
            DBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));
        }

        public Project GetProject(string id) => DBContext.Projects.FirstOrDefault(project => project.Id.ToString() == id);

        public bool IsUserAssignedToProject(ApplicationUser applicationUser, Project project) => project?.Users.Any(user => user?.Id == applicationUser.Id) ?? false;
        public bool IsUserAssignedToProject(string userId, Project project) => project?.Users.FirstOrDefault(user => user?.Id == userId) != null;
        public bool IsUserAssignedToProject(string userId, string projectId) => GetProject(projectId)?.Users.FirstOrDefault(user => user?.Id == userId) != null;


        public List<Project> GetUserProjects(string userId) => DBContext.Projects
            .Where(project => project.Users.Any(user => userId == user.Id))
            .ToList();

        public List<Project> GetAllProjects() => DBContext.Projects.ToList();
    }
}