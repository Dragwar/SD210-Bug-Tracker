﻿using BugTracker.Models;
using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool UnassignUserFromProject(string userId, string projectId)
        {
            try
            {
                Project foundProject = GetProject(projectId);
                bool didUserGetAdded = foundProject.Users.RemoveAll(user => user.Id == userId) == 1;
                if (didUserGetAdded && foundProject.Users.Any(user => user.Id == userId))
                {
                    DBContext.SaveChanges();
                }
                return didUserGetAdded && foundProject.Users.Any(user => user.Id == userId);
            }
            catch
            {
                return false;
            }
        }
        public bool AssignUserToProject(ApplicationUser applicationUser, string projectId)
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
        public List<Project> GetUserProjects(string userId) => DBContext.Projects
            .Where(project => project.Users.Any(user => userId == user.Id))
            .ToList();
        public bool IsProjectNameAlreadyTaken(string projectName) => DBContext.Projects.Any(project => project.Name.ToLower() == projectName.ToLower());
        public List<Project> GetAllProjects() => DBContext.Projects.ToList();
    }
}