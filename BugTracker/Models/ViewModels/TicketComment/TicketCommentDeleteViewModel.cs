using System;
using BugTracker.Models.Domain;

namespace BugTracker.Models.ViewModels.TicketComment
{
    public class TicketCommentDeleteViewModel
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public Guid TicketId { get; set; }
        public string TicketName { get; set; }

        public static TicketCommentDeleteViewModel CreateNewViewModel(Domain.TicketComment ticketComment)
        {
            if (ticketComment == null)
            {
                throw new ArgumentNullException(nameof(ticketComment));
            }

            return new TicketCommentDeleteViewModel()
            {
                Id = ticketComment.Id,
                Comment = ticketComment.Comment,
                TicketId = ticketComment?.Ticket.Id ?? throw new ArgumentException("TicketComment wasn't attached to ticket"),
                TicketName = ticketComment?.Ticket.Title ?? throw new ArgumentException("TicketComment wasn't attached to ticket"),
            };
        }
    }
}