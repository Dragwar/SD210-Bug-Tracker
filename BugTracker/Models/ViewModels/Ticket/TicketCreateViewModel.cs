using BugTracker.MyHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels.Ticket
{
    public class TicketCreateViewModel
    {
        [Required]
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

        public string AuthorId { get; set; }
    }
}