using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        //[NotMapped]
        //public TicketTypesEnum Type { get; set; }

        //[NotMapped]
        //public TicketPrioritiesEnum Priority { get; set; }

        //[NotMapped]
        //public TicketStatusesEnum Status { get; set; }

        //[Column("Type")]
        //public string TypeString { get => Type.ToString(); private set => Type = value.ParseEnum<TicketTypesEnum>(); }

        //[Column("Priority")]
        //public string PriorityString { get => Priority.ToString(); private set => Priority = value.ParseEnum<TicketPrioritiesEnum>(); }

        //[Column("Status")]
        //public string StatusString { get => Status.ToString(); private set => Status = value.ParseEnum<TicketStatusesEnum>(); }



        //public virtual Project Project { get; set; }
        //public Guid ProjectId { get; set; }

        //public virtual ApplicationUser Author { get; set; }
        //public string AuthorId { get; set; }
        //public virtual ApplicationUser AssignedUser { get; set; }
        //public string AssignedUserId { get; set; }

        public Ticket()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }
    }
}