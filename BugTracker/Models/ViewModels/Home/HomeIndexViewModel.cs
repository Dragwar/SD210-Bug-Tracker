using BugTracker.Models.ViewModels.Project;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BugTracker.Models.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        public int TotalProjectCount { get; set; }
        public List<ProjectIndexViewModel> LatestProjects { get; set; }

        public List<IdentityRole> Roles { get; set; }

        public static HomeIndexViewModel CreateNewViewModel(ApplicationUser applicationUser, ApplicationDbContext dbContext, int latestProjectIntakeLimit = 3)
        {
            if (applicationUser == null)
            {
                throw new ArgumentNullException(nameof(applicationUser));
            }
            List<IdentityRole> roles = new UserRoleRepository(dbContext).GetUserRoles(applicationUser.Id);
            ProjectRepository repo = new ProjectRepository(dbContext);

            List<Project.ProjectIndexViewModel> latestProjects = repo
                .GetUserProjects(applicationUser.Id)?
                .OrderByDescending(project => project?.DateUpdated ?? project.DateCreated)
                .Take(latestProjectIntakeLimit)
                .Select(project => ProjectIndexViewModel.CreateNewViewModel(project))
                .ToList() ?? new List<ProjectIndexViewModel>();
            try
            {
                return new HomeIndexViewModel()
                {
                    UserId = string.IsNullOrWhiteSpace(applicationUser.Id) ? throw new ArgumentNullException() : applicationUser.Id,
                    DisplayName = string.IsNullOrWhiteSpace(applicationUser.DisplayName) ? throw new ArgumentNullException() : applicationUser.DisplayName,
                    Email = string.IsNullOrWhiteSpace(applicationUser.Email) ? throw new ArgumentNullException() : applicationUser.Email,
                    TotalProjectCount = repo.GetUserProjects(applicationUser.Id)?.Count ?? 0,
                    LatestProjects = latestProjects?.Any() ?? false ? latestProjects : new List<ProjectIndexViewModel>(),
                    Roles = (roles?.Any() ?? false) ? roles : new List<IdentityRole>(),
                };
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"{e.Message}");
            }
            catch
            {
                throw new Exception("Something bad happened");
            }
        }
    }
}