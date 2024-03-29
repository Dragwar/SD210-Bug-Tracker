﻿using System;

namespace BugTracker.Models.Domain
{
    public class TicketComment
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Ticket Ticket { get; set; }
        public Guid TicketId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public TicketComment()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }

        public override string ToString() => $"TicketComment - Comment: {Comment}";
    }
}