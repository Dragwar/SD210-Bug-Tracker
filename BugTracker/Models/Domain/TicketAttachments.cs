using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models.Domain
{
    public class TicketAttachments
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Ticket Ticket { get; set; }
        public Guid TicketId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }


        public TicketAttachments()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }

        public override string ToString() => $"Ticket Attachment - Description: {Description}";
    }
}