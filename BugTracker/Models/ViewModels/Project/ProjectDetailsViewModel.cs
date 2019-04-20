using BugTracker.Models.ViewModels.Ticket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BugTracker.Models.ViewModels.Project
{
    public class ProjectDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<HelperUserViewModel> Users { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        [Display(Name = "Total Users")]
        public int UsersCount => Users?.Count ?? 0;

        [Display(Name = "Total Tickets")]
        public int TicketCount { get; set; }
        public List<TicketIndexViewModel> Tickets { get; set; }

        public string CurrentUserId { get; set; }

        public static ProjectDetailsViewModel CreateNewViewModel(string currentUserId, Domain.Project project, ApplicationDbContext dbContext)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            try
            {
                return new ProjectDetailsViewModel()
                {
                    Id = project.Id == null ? throw new ArgumentNullException() : project.Id,
                    Name = string.IsNullOrWhiteSpace(project.Name) ? throw new ArgumentNullException() : project.Name,
                    Users = project.Users?.Select(user => HelperUserViewModel.CreateNewViewModel(user, dbContext)).ToList() ?? throw new ArgumentNullException(),
                    TicketCount = project.Tickets?.Count ?? 0,
                    Tickets = project.Tickets?.Select(ticket => TicketIndexViewModel.CreateNewViewModel(currentUserId, ticket)).ToList() ?? new List<TicketIndexViewModel>(),
                    DateCreated = project.DateCreated == null ? throw new ArgumentNullException() : project.DateCreated,
                    DateUpdated = project.DateUpdated,
                    CurrentUserId = currentUserId,
                };
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"{e.Message}");
            }
            catch (Exception e)
            {
                throw new Exception($"Something went wrong\n {e.Message}");
            }
        }
    }
}