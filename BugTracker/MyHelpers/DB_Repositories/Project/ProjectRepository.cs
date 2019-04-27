using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BugTracker.Models;
using BugTracker.Models.Domain;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class ProjectRepository
    {
        private readonly ApplicationDbContext DBContext;

        public ProjectRepository(ApplicationDbContext dBContext) => DBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));

        public Project GetProject(Guid id) => DBContext.Projects.FirstOrDefault(project => !project.IsArchived && project.Id == id);

        /// <summary>Will throw Exception if project wasn't found</summary>
        public void MarkProjectAsArchived(Guid id) => DBContext.Projects.First(project => project.Id == id).IsArchived = true;
        public bool DoesProjectExist(Guid id) => DBContext.Projects.Any(project => !project.IsArchived && project.Id == id);
        public bool IsProjectArchived(Guid id) => DBContext.Projects.Any(project => project.IsArchived && project.Id == id);
        public bool IsUserAssignedToProject(ApplicationUser applicationUser, Project project) => project?.Users.Any(user => user?.Id == applicationUser.Id) ?? false;
        public bool IsUserAssignedToProject(string userId, Project project) => project?.Users.FirstOrDefault(user => user?.Id == userId) != null;
        public bool IsUserAssignedToProject(string userId, Guid projectId) => GetProject(projectId)?.Users.FirstOrDefault(user => user?.Id == userId) != null;
        public bool UnassignUserFromProject(string userId, Guid projectId)
        {
            try
            {
                Project foundProject = GetProject(projectId);
                bool didUserGetAdded = foundProject.Users.RemoveAll(user => user.Id == userId) == 1;
                bool isUserStillInProject = foundProject.Users.Any(user => user.Id == userId);
                if (didUserGetAdded && !isUserStillInProject)
                {
                    DBContext.SaveChanges();
                }
                return didUserGetAdded && !isUserStillInProject;
            }
            catch
            {
                return false;
            }
        }
        public bool AssignUserToProject(ApplicationUser applicationUser, Guid projectId)
        {
            try
            {
                Project foundProject = GetProject(projectId);
                foundProject?.Users.Add(applicationUser);
                if (foundProject == null || !foundProject.Users.Contains(applicationUser))
                {
                    return false;
                }
                else if (foundProject.Users.Contains(applicationUser))
                {
                    DBContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public IQueryable<Project> GetUserProjects(string userId) => DBContext.Projects
            .Where(project => !project.IsArchived && project.Users.Any(user => userId == user.Id))
            .AsQueryable();
        public bool IsProjectNameAlreadyTaken(string projectName) => DBContext.Projects
            .Any(project => !project.IsArchived && project.Name.ToLower() == projectName.ToLower());
        public bool IsProjectNameAlreadyTaken(string projectName, Guid projectId) => DBContext.Projects
            .Any(project => !project.IsArchived && project.Name.ToLower() == projectName.ToLower() && project.Id != projectId);
        public IQueryable<Project> GetAllProjects() => DBContext.Projects.Where(project => !project.IsArchived).AsQueryable();

        [Obsolete("You shouldn't really need to user this method")]
        public IQueryable<Project> GetAllArchivedProjects() => DBContext.Projects.Where(project => project.IsArchived).AsQueryable();
    }
}