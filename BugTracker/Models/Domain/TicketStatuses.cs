using BugTracker.MyHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketStatuses
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }


        [NotMapped]
        public TicketStatusesEnum Status { get; set; }

        [Column("Status")]
        public string StatusString { get => Status.ToString(); set => Status = value.ParseEnum<TicketStatusesEnum>(); }

        public override string ToString() => $"Id: {Id} - Status: {StatusString}";
    }
}