using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace BugTracker.Models.ViewModels.TicketAttachment
{
    public class TicketAttachmentCreateViewModel
    {
        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase Media { get; set; }

        public Guid TicketId { get; set; }
        public string TicketTitle { get; set; }

        public static TicketAttachmentCreateViewModel CreateNewViewModel(Domain.Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }
            
            return new TicketAttachmentCreateViewModel()
            {
                TicketId = ticket.Id,
                TicketTitle = ticket.Title,
            };
        }
    }
}