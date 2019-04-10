using System;
using System.Collections.Generic;

namespace BugTracker.Models.Domain
{
    public class Project
    {
        public Guid Id { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }

        public virtual List<Ticket> Tickets { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public Project()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
            Users = new List<ApplicationUser>();
            Tickets = new List<Ticket>();
        }
    }
}