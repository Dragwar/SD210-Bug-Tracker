using System;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models.ViewModels.TicketComment
{
    public class TicketCommentCreateViewModel : ITicketCommentCreateEditViewModel
    {
        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string Comment { get; set; }

        [Required]
        public Guid TicketId { get; set; }
        public string TicketTitle { get; set; }

        [Required]
        public string UserId { get; set; }

        public static TicketCommentCreateViewModel CreateNewViewModel(Domain.Ticket ticket, string userId)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            return new TicketCommentCreateViewModel()
            {
                TicketId = ticket?.Id ?? throw new ArgumentException(),
                TicketTitle = ticket?.Title ?? throw new ArgumentException(),
                UserId = userId
            };
        }
    }
}