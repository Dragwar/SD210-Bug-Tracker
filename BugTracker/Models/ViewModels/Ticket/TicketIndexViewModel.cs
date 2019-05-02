using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace BugTracker.Models.ViewModels.Ticket
{
    public class TicketIndexViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        [Display(Name = "Created")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Updated")]
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

        [Display(Name = "Author")]
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }

        [Display(Name = "Assigned User")]
        public string AssignedUserName { get; set; }
        public string AssignedUserId { get; set; }

        public bool IsCurrentUserTheAuthorOrIsAssigned { get; set; }
        public bool IsWatching { get; set; }

        public static TicketIndexViewModel CreateNewViewModel(string currentUserId, Domain.Ticket ticket, [Optional] bool? isWatching)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException();
            }

            bool isCurrentUserTheAuthorOrIsAssigned = ticket.AuthorId == currentUserId;

            if (!isCurrentUserTheAuthorOrIsAssigned)
            {
                isCurrentUserTheAuthorOrIsAssigned = ticket?.AssignedUserId != null ? ticket.AssignedUserId == currentUserId : false;
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
                    AssignedUserId = ticket?.AssignedUserId,
                    AssignedUserName = ticket?.AssignedUser?.UserName,
                    AuthorId = ticket.AuthorId,
                    AuthorName = ticket.Author.UserName,
                    ProjectId = ticket.ProjectId,
                    ProjectName = ticket.Project.Name,
                    IsCurrentUserTheAuthorOrIsAssigned = isCurrentUserTheAuthorOrIsAssigned,
                    IsWatching = isWatching ?? false,
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Something went wrong\n {e.Message}");
            }
        }
    }
}