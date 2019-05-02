using System;

namespace BugTracker.Models.Domain
{
    public class TicketNotification
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }


        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }

        public virtual Ticket Ticket { get; set; }
        public Guid TicketId { get; set; }
        public string TicketTitle { get; set; }

        public TicketNotification()
        {
            DateCreated = DateTime.Now;
        }
    }
}