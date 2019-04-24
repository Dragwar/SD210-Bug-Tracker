using BugTracker.MyHelpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models.Domain
{
    public class TicketTypes
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [NotMapped]
        public TicketTypesEnum Type { get; set; }

        
        [Column("Type")]
        public string TypeString { get => Type.ToString(); set => Type = value.ParseEnum<TicketTypesEnum>(); }

        public virtual List<Ticket> Tickets { get; set; }

        public override string ToString() => $"Id: {Id} - Type: {TypeString}";
    }
}