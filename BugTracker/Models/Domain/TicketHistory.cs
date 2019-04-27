using System;

namespace BugTracker.Models.Domain
{
    public class TicketHistory
    {
        public Guid Id { get; set; }
        public DateTime DateChanged { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public Guid TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }

        public TicketHistory()
        {
            Id = Guid.NewGuid();
            DateChanged = DateTime.Now;
        }

        public override string ToString() => $"TicketHistory: {$"{nameof(Property)} - {Property}"}, {$"{nameof(OldValue)} - {OldValue}"}, {$"{nameof(NewValue)}-{NewValue}"}";
    }
}