using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.TicketHistory
{
    public class TicketHistoryIndexViewModel
    {
        public Guid Id { get; set; }
        public DateTime DateChanged { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public HelperUserViewModel UserWhoMadeTheChange { get; set; }

        //public override string ToString() => $"(Changed) {Property}: Old - {OldValue}, New - {NewValue}, ChangeMadeBy - {UserWhoMadeTheChange.Email}";

        public static TicketHistoryIndexViewModel CreateNewViewModel(ApplicationDbContext dbContext, Domain.TicketHistory ticketHistory)
        {
            if (ticketHistory == null)
            {
                throw new ArgumentNullException(nameof(ticketHistory));
            }

            return new TicketHistoryIndexViewModel()
            {
                Id = ticketHistory.Id,
                DateChanged = ticketHistory.DateChanged,
                Property = ticketHistory.Property,
                NewValue = ticketHistory.NewValue,
                OldValue = ticketHistory.OldValue,
                UserWhoMadeTheChange = HelperUserViewModel.CreateNewViewModel(ticketHistory.User, dbContext),
            };
        }
    }
}