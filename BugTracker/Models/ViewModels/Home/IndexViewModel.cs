﻿using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BugTracker.Models.ViewModels.Home
{
    public class IndexViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        public int TotalProjectCount { get; set; }
        public List<Project.IndexViewModel> LatestProjects { get; set; }

        public List<IdentityRole> Roles { get; set; }

        public static IndexViewModel CreateNewViewModel(ApplicationUser applicationUser, ApplicationDbContext dbContext, int latestProjectIntakeLimit = 3)
        {
            if (applicationUser == null)
            {
                throw new ArgumentNullException(nameof(applicationUser));
            }
            List<IdentityRole> roles = new UserRoleRepository(dbContext).GetUserRoles(applicationUser.Id);
            ProjectRepository repo = new ProjectRepository(dbContext);

            List<Project.IndexViewModel> latestProjects = repo
                .GetUserProjects(applicationUser.Id)?
                .OrderByDescending(project => project?.DateUpdated ?? project.DateCreated)
                .Take(latestProjectIntakeLimit)
                .Select(project => Project.IndexViewModel.CreateNewViewModel(project))
                .ToList() ?? new List<Project.IndexViewModel>();
            try
            {
                return new IndexViewModel()
                {
                    UserId = string.IsNullOrWhiteSpace(applicationUser.Id) ? throw new ArgumentNullException() : applicationUser.Id,
                    DisplayName = string.IsNullOrWhiteSpace(applicationUser.DisplayName) ? throw new ArgumentNullException() : applicationUser.DisplayName,
                    Email = string.IsNullOrWhiteSpace(applicationUser.Email) ? throw new ArgumentNullException() : applicationUser.Email,
                    TotalProjectCount = repo.GetUserProjects(applicationUser.Id)?.Count ?? 0,
                    LatestProjects = latestProjects?.Any() ?? false ? latestProjects : new List<Project.IndexViewModel>(),
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