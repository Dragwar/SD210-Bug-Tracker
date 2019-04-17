using BugTracker.MyHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels.Ticket
{
    public class TicketEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 4)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public TicketPrioritiesEnum? Priority { get; set; }


        [Required]
        public TicketTypesEnum? Type { get; set; }

        public TicketStatusesEnum? Status { get; set; }

        public List<SelectListItem> Projects { get; set; }

        [Required]
        [Display(Name = "Project")]
        public Guid ProjectId { get; set; }

        public List<SelectListItem> DeveloperUsers { get; set; }

        [Display(Name = "Assigned User")]
        public string DeveloperId { get; set; }

        public static TicketEditViewModel CreateNewViewModel(Domain.Ticket ticket, List<SelectListItem> developersToChooseFrom, List<SelectListItem> projectsToChooseFrom)
        {
            if (ticket == null || developersToChooseFrom == null || projectsToChooseFrom == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                return new TicketEditViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Priority = (TicketPrioritiesEnum)ticket.PriorityId,
                    Status = (TicketStatusesEnum)ticket.StatusId,
                    Type = (TicketTypesEnum)ticket.TypeId,
                    ProjectId = ticket.ProjectId,
                    Projects = projectsToChooseFrom,
                    DeveloperUsers = developersToChooseFrom,
                    DeveloperId = ticket.AssignedUserId,
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
    }
}