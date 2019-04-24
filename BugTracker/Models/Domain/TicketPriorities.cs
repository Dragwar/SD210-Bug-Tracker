using BugTracker.MyHelpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models.Domain
{
    public class TicketPriorities
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [NotMapped]
        public TicketPrioritiesEnum Priority { get; set; }

        [Column("Priority")]
        public string PriorityString { get => Priority.ToString(); set => Priority = value.ParseEnum<TicketPrioritiesEnum>(); }

        public virtual List<Ticket> Tickets { get; set; }

        public override string ToString() => $"Id: {Id} - Priority: {PriorityString}";
    }
}