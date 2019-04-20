using System;
using BugTracker.Models.Domain;

namespace BugTracker.Models.ViewModels.TicketComment
{
    public class TicketCommentIndexViewModel
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid TicketId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }

        public static TicketCommentIndexViewModel CreateNewViewModel(TicketComments ticketComment)
        {
            if (ticketComment == null)
            {
                throw new ArgumentNullException(nameof(ticketComment));
            }

            return new TicketCommentIndexViewModel()
            {
                Id = ticketComment.Id,
                Comment = ticketComment.Comment,
                DateCreated = ticketComment.DateCreated,
                TicketId = ticketComment.TicketId,
                UserId = ticketComment.UserId,
                UserEmail = ticketComment.User.Email,
            };
        }
    }
}