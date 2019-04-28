using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Models.ViewModels.Project;
using BugTracker.Models.ViewModels.Ticket;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity.EntityFramework;

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
        public int AllProjectCount { get; set; }
        public int AllOpenTicketsCount { get; set; }
        public int AllResovledTicketsCount { get; set; }
        public int AllRejectedTicketsCount { get; set; }
        public int AllTicketsCount => AllOpenTicketsCount + AllResovledTicketsCount + AllRejectedTicketsCount;


        public static HomeIndexViewModel CreateNewViewModel(
            ApplicationUser applicationUser,
            ApplicationDbContext dbContext,
            int latestProjectIntakeLimit = 3,
            int latestCreatedTicketIntakeLimit = 3,
            int latestAssignedTicketIntakeLimit = 3)
        {
            if (applicationUser == null)
            {
                throw new ArgumentNullException(nameof(applicationUser));
            }

            List<IdentityRole> roles = new UserRoleRepository(dbContext).GetUserRoles(applicationUser.Id);
            ProjectRepository projectRepository = new ProjectRepository(dbContext);

            List<ProjectIndexViewModel> latestProjects = projectRepository
                .GetUserProjects(applicationUser.Id)?
                .ToList()
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
                   .Select(ticket => TicketIndexViewModel.CreateNewViewModel(applicationUser.Id, ticket))
                   .ToList() ?? new List<TicketIndexViewModel>();
            }
            else
            {
                //! if not (Submitter)
                latestCreatedTickets = new List<TicketIndexViewModel>();
            }

            if (roles.Any(role => role.Name == nameof(UserRolesEnum.Developer)))
            {
                //! get assigned tickets (Developer)
                numberOfAssignedTickets = applicationUser.AssignedTickets?.Count ?? 0;
                latestAssignedTickets = applicationUser.AssignedTickets?
                    .OrderByDescending(ticket => ticket?.DateUpdated ?? ticket.DateCreated)
                    .Take(latestAssignedTicketIntakeLimit)
                    .Select(ticket => TicketIndexViewModel.CreateNewViewModel(applicationUser.Id, ticket))
                    .ToList() ?? new List<TicketIndexViewModel>();
            }
            else
            {
                //! if not (Developer)
                latestAssignedTickets = new List<TicketIndexViewModel>();
            }

            TicketRepository ticketRepository = new TicketRepository(dbContext);
            try
            {
                return new HomeIndexViewModel()
                {
                    UserId = string.IsNullOrWhiteSpace(applicationUser.Id) ? throw new ArgumentNullException() : applicationUser.Id,
                    DisplayName = string.IsNullOrWhiteSpace(applicationUser.DisplayName) ? throw new ArgumentNullException() : applicationUser.DisplayName,
                    Email = string.IsNullOrWhiteSpace(applicationUser.Email) ? throw new ArgumentNullException() : applicationUser.Email,
                    TotalProjectCount = projectRepository.GetUserProjects(applicationUser.Id)?.Count() ?? 0,
                    LatestProjects = latestProjects?.Any() ?? false ? latestProjects : new List<ProjectIndexViewModel>(),
                    Roles = (roles?.Any() ?? false) ? roles : new List<IdentityRole>(),
                    TotalCreatedTicketCount = numberOfCreatedTickets,
                    TotalAssignedTicketCount = numberOfAssignedTickets,
                    LatestCreatedTickets = latestCreatedTickets,
                    LatestAssignedTickets = latestAssignedTickets,
                    AllProjectCount = projectRepository.GetAllProjects().Count(),

                    //! NOTE: This depends on the table primary keys matching with the enum int value
                    AllOpenTicketsCount = ticketRepository.GetAllTickets().Count(ticket => ticket.StatusId == (int)TicketStatusesEnum.Open),
                    AllResovledTicketsCount = ticketRepository.GetAllTickets().Count(ticket => ticket.StatusId == (int)TicketStatusesEnum.Resolved),
                    AllRejectedTicketsCount = ticketRepository.GetAllTickets().Count(ticket => ticket.StatusId == (int)TicketStatusesEnum.Rejected),
                };
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"{e.Message}");
            }
            catch (Exception e)
            {
                throw new Exception($"Something bad happened\n {e}");
            }
        }
    }
}