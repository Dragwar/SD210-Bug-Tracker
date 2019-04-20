using System;
using TicketIndexViewModel = BugTracker.Models.ViewModels.Ticket.TicketIndexViewModel;

namespace BugTracker.Models.ViewModels.TicketAttachment
{
    public class TicketAttachmentIndexViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public DateTime DateCreated { get; set; }

        public TicketIndexViewModel Ticket { get; set; }
        public Guid TicketId { get; set; }

        public HelperUserViewModel User { get; set; }
        public string UserId { get; set; }

        public static TicketAttachmentIndexViewModel CreateNewViewModel(string currentUserId, Domain.TicketAttachment ticketAttachment, ApplicationDbContext dbContext)
        {
            if (ticketAttachment == null)
            {
                throw new ArgumentNullException(nameof(ticketAttachment));
            }

            TicketIndexViewModel ticket = TicketIndexViewModel.CreateNewViewModel(currentUserId, ticketAttachment.Ticket) ?? throw new ArgumentException();
            HelperUserViewModel user = HelperUserViewModel.CreateNewViewModel(ticketAttachment.User, dbContext) ?? throw new ArgumentException();

            return new TicketAttachmentIndexViewModel()
            {
                Id = ticketAttachment.Id,
                Description = ticketAttachment.Description,
                FilePath = ticketAttachment.FilePath,
                FileUrl = ticketAttachment.FileUrl,
                DateCreated = ticketAttachment.DateCreated,

                User = user,
                UserId = user.Id,

                Ticket = ticket,
                TicketId = ticket.Id,
            };
        }
    }
}