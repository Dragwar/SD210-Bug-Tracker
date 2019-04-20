using System;

namespace BugTracker.Models.ViewModels.TicketComment
{
    public interface ITicketCommentCreateEditViewModel
    {
        Guid TicketId { get; set; }
        string UserId { get; set; }
        string Comment { get; set; }
        string TicketName { get; set; }
    }
}
