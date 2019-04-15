using BugTracker.Models.ViewModels.Project;
using BugTracker.Models.ViewModels.Ticket;
using BugTracker.MyHelpers;
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

        public int TotalCreatedTicketCount { get; set; }
        public int TotalAssignedTicketCount { get; set; }
        public int TotalProjectCount { get; set; }
        public List<ProjectIndexViewModel> LatestProjects { get; set; }
        public List<TicketIndexViewModel> LatestCreatedTickets { get; set; }
        public List<TicketIndexViewModel> LatestAssignedTickets { get; set; }

        public List<IdentityRole> Roles { get; set; }

        public static HomeIndexViewModel CreateNewViewModel(ApplicationUser applicationUser, ApplicationDbContext dbContext, int latestProjectIntakeLimit = 3, int latestCreatedTicketIntakeLimit = 3, int latestAssignedTicketIntakeLimit = 3)
        {
            if (applicationUser == null)
            {
                throw new ArgumentNullException(nameof(applicationUser));
            }

            List<IdentityRole> roles = new UserRoleRepository(dbContext).GetUserRoles(applicationUser.Id);
            ProjectRepository repo = new ProjectRepository(dbContext);

            List<ProjectIndexViewModel> latestProjects = repo
                .GetUserProjects(applicationUser.Id)?
                .OrderByDescending(project => project?.DateUpdated ?? project.DateCreated)
                .Take(latestProjectIntakeLimit)
                .Select(project => ProjectIndexViewModel.CreateNewViewModel(project))
                .ToList() ?? new List<ProjectIndexViewModel>();

            int numberOfCreatedTickets = 0;
            int numberOfAssignedTickets = 0;

            List<TicketIndexViewModel> latestCreatedTickets;
            List<TicketIndexViewModel> latestAssignedTickets;

            if (roles.Any(role => role.Name == nameof(UserRolesEnum.Submitter)))
            {
                //! get created tickets (Submitter)
                numberOfCreatedTickets = applicationUser.CreatedTickets?.Count ?? 0;
                latestCreatedTickets = applicationUser.CreatedTickets?
                   .OrderByDescending(ticket => ticket?.DateUpdated ?? ticket.DateCreated)
                   .Take(latestCreatedTicketIntakeLimit)
                   .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                   .ToList() ?? new List<TicketIndexViewModel>();
                latestAssignedTickets = new List<TicketIndexViewModel>();
            }
            else if (roles.Any(role => role.Name == nameof(UserRolesEnum.Developer)))
            {
                //! get assigned tickets (Developer)
                numberOfAssignedTickets = applicationUser.AssignedTickets?.Count ?? 0;
                latestAssignedTickets = applicationUser.AssignedTickets?
                    .OrderByDescending(ticket => ticket?.DateUpdated ?? ticket.DateCreated)
                    .Take(latestAssignedTicketIntakeLimit)
                    .Select(ticket => TicketIndexViewModel.CreateViewModel(ticket))
                    .ToList() ?? new List<TicketIndexViewModel>();
                latestCreatedTickets = new List<TicketIndexViewModel>();
            }
            else
            {
                //! if not (Developer) or (Submitter)
                latestCreatedTickets = new List<TicketIndexViewModel>();
                latestAssignedTickets = new List<TicketIndexViewModel>();
            }


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
                    TotalCreatedTicketCount = numberOfCreatedTickets,
                    TotalAssignedTicketCount = numberOfAssignedTickets,
                    LatestCreatedTickets = latestCreatedTickets,
                    LatestAssignedTickets = latestAssignedTickets,
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