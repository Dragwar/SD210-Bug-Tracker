using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.TicketAttachment
{
    public class TicketAttachmentDeleteViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "File Name")]
        public string FileName { get; set; }
        public string Description { get; set; }
        public Guid TicketId { get; set; }
        public string TicketName { get; set; }

        public static TicketAttachmentDeleteViewModel CreateNewViewModel(Domain.TicketAttachment ticketAttachment)
        {
            if (ticketAttachment == null)
            {
                throw new ArgumentNullException(nameof(ticketAttachment));
            }

            return new TicketAttachmentDeleteViewModel()
            {
                Id = ticketAttachment.Id,
                FileName = Path.GetFileName(ticketAttachment.FilePath),
                Description = ticketAttachment.Description,
                TicketId = ticketAttachment?.Ticket.Id ?? throw new ArgumentException("TicketAttachment wasn't attached to ticket"),
                TicketName = ticketAttachment?.Ticket.Title ?? throw new ArgumentException("TicketAttachment wasn't attached to ticket"),
            };
        }
    }
}