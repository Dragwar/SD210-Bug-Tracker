using System;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models.ViewModels.Ticket
{
    public class TicketIndexViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }

        [Display(Name = "# Attachment")]
        public int AttachmentCount { get; set; }

        [Display(Name = "# Comment")]
        public int CommentCount { get; set; }

        [Display(Name = "Project")]
        public string ProjectName { get; set; }
        public Guid ProjectId { get; set; }

        //[Display(Name = "Author")]
        //public string AuthorName { get; set; }
        //public string AuthorId { get; set; }

        //[Display(Name = "Assigned User")]
        //public string AssignedUserName { get; set; }
        //public string AssignedUserId { get; set; }

        public static TicketIndexViewModel CreateViewModel(Domain.Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                return new TicketIndexViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Priority = ticket.Priority.PriorityString,
                    Type = ticket.Type.TypeString,
                    Status = ticket.Status.StatusString,
                    AttachmentCount = ticket.Attachments.Count,
                    CommentCount = ticket.Comments.Count,
                    DateCreated = ticket.DateCreated,
                    DateUpdated = ticket.DateUpdated,
                    //AssignedUserId = ticket.AssignedUserId,
                    //AssignedUserName = ticket.AssignedUser.UserName,
                    //AuthorId = ticket.AuthorId,
                    //AuthorName = ticket.Author.UserName,
                    ProjectId = ticket.ProjectId,
                    ProjectName = ticket.Project.Name,
                };
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}