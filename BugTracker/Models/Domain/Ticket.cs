using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;
using BugTracker.MyHelpers;

namespace BugTracker.Models.Domain
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }



        public virtual TicketPriorities Priority { get; set; }
        public int PriorityId { get; set; }

        public virtual TicketStatuses Status { get; set; }
        public int StatusId { get; set; }

        public virtual TicketTypes Type { get; set; }
        public int TypeId { get; set; }

        public virtual List<TicketAttachments> Attachments { get; set; }

        public virtual List<TicketComments> Comments { get; set; }

        public virtual Project Project { get; set; }
        public Guid ProjectId { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public string AuthorId { get; set; }
        public virtual ApplicationUser AssignedUser { get; set; }
        public string AssignedUserId { get; set; }

        public Ticket()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
            Attachments = new List<TicketAttachments>();
            Comments = new List<TicketComments>();
        }
    }
}