using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.Project
{
    public class IndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        [Display(Name = "Total Users")]
        public int UsersCount { get; set;/* get => UsersCount; set => UsersCount = value >= 0 ? value : 0; */}

        [Display(Name = "Total Tickets")]
        public int TicketCount { get; set;/*get => TicketCount; set => TicketCount = value < 0 ? 0 : value;*/ } // TODO: Change in second deliverable
        public static IndexViewModel CreateNewViewModel(BugTracker.Models.Domain.Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            try
            {
                return new IndexViewModel()
                {
                    Id = project.Id == null ? throw new ArgumentNullException() : project.Id,
                    Name = string.IsNullOrWhiteSpace(project.Name) ? throw new ArgumentNullException() : project.Name,
                    UsersCount = project?.Users?.Count ?? 0,
                    TicketCount = 0, // TODO: Change in second deliverable
                    DateCreated = project.DateCreated == null ? throw new ArgumentNullException() : project.DateCreated,
                    DateUpdated = project.DateUpdated,
                };
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"{e.Message}");
            }
        }
    }
}