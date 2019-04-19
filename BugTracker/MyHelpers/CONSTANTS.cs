using BugTracker.Models.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace BugTracker.MyHelpers
{
    public static class CONSTANTS
    {
        //! NOTE: i tried using a constructor for this but EF needs a empty constructor for DB
        #region TicketPriorities
        #region CONSTANT Ticket Priorities
        private static readonly TicketPriorities TicketPriorityLow = new TicketPriorities()
        {
            Id = (int)TicketPrioritiesEnum.Low,
            Priority = TicketPrioritiesEnum.Low,
            PriorityString = TicketPrioritiesEnum.Low.ToString(),
        };
        private static readonly TicketPriorities TicketPriorityMedium = new TicketPriorities()
        {
            Id = (int)TicketPrioritiesEnum.Medium,
            Priority = TicketPrioritiesEnum.Medium,
            PriorityString = TicketPrioritiesEnum.Medium.ToString(),
        };
        private static readonly TicketPriorities TicketPriorityHigh = new TicketPriorities()
        {
            Id = (int)TicketPrioritiesEnum.High,
            Priority = TicketPrioritiesEnum.High,
            PriorityString = TicketPrioritiesEnum.High.ToString(),
        };
        #endregion

        private static IDictionary<TicketPrioritiesEnum, TicketPriorities> _TicketPriorites => new Dictionary<TicketPrioritiesEnum, TicketPriorities>()
        {
            [TicketPrioritiesEnum.Low] = TicketPriorityLow,
            [TicketPrioritiesEnum.Medium] = TicketPriorityMedium,
            [TicketPrioritiesEnum.High] = TicketPriorityHigh,
        };

        public static ReadOnlyDictionary<TicketPrioritiesEnum, TicketPriorities> TicketPriorites => new ReadOnlyDictionary<TicketPrioritiesEnum, TicketPriorities>(_TicketPriorites);
        #endregion

        #region TicketStatuses
        #region CONSTANT Ticket Statuses
        private static readonly TicketStatuses TicketStatusOpen = new TicketStatuses()
        {
            Id = (int)TicketStatusesEnum.Open,
            Status = TicketStatusesEnum.Open,
            StatusString = TicketStatusesEnum.Open.ToString(),
        };
        private static readonly TicketStatuses TicketStatusResolved = new TicketStatuses()
        {
            Id = (int)TicketStatusesEnum.Resolved,
            Status = TicketStatusesEnum.Resolved,
            StatusString = TicketStatusesEnum.Resolved.ToString(),
        };
        private static readonly TicketStatuses TicketStatusRejected = new TicketStatuses()
        {
            Id = (int)TicketStatusesEnum.Rejected,
            Status = TicketStatusesEnum.Rejected,
            StatusString = TicketStatusesEnum.Rejected.ToString(),
        };
        #endregion
        private static IDictionary<TicketStatusesEnum, TicketStatuses> _TicketStatuses => new Dictionary<TicketStatusesEnum, TicketStatuses>()
        {
            [TicketStatusesEnum.Open] = TicketStatusOpen,
            [TicketStatusesEnum.Resolved] = TicketStatusResolved,
            [TicketStatusesEnum.Rejected] = TicketStatusRejected,
        };

        public static ReadOnlyDictionary<TicketStatusesEnum, TicketStatuses> TicketStatuses => new ReadOnlyDictionary<TicketStatusesEnum, TicketStatuses>(_TicketStatuses);
        #endregion

        #region TicketTypes
        #region CONSTANT Ticket Types
        private static readonly TicketTypes TicketTypeBug = new TicketTypes()
        {
            Id = (int)TicketTypesEnum.Bug,
            Type = TicketTypesEnum.Bug,
            TypeString = TicketTypesEnum.Bug.ToString(),
        };
        private static readonly TicketTypes TicketTypeFeature = new TicketTypes()
        {
            Id = (int)TicketTypesEnum.Feature,
            Type = TicketTypesEnum.Feature,
            TypeString = TicketTypesEnum.Feature.ToString(),
        };
        private static readonly TicketTypes TicketTypeDatabase = new TicketTypes()
        {
            Id = (int)TicketTypesEnum.Database,
            Type = TicketTypesEnum.Database,
            TypeString = TicketTypesEnum.Database.ToString(),
        };
        private static readonly TicketTypes TicketTypeSupport = new TicketTypes()
        {
            Id = (int)TicketTypesEnum.Support,
            Type = TicketTypesEnum.Support,
            TypeString = TicketTypesEnum.Support.ToString(),
        };
        #endregion
        private static IDictionary<TicketTypesEnum, TicketTypes> _TicketTypes => new Dictionary<TicketTypesEnum, TicketTypes>()
        {
            [TicketTypesEnum.Bug] = TicketTypeBug,
            [TicketTypesEnum.Feature] = TicketTypeFeature,
            [TicketTypesEnum.Database] = TicketTypeDatabase,
            [TicketTypesEnum.Support] = TicketTypeSupport,
        };

        public static ReadOnlyDictionary<TicketTypesEnum, TicketTypes> TicketTypes => new ReadOnlyDictionary<TicketTypesEnum, TicketTypes>(_TicketTypes);
        #endregion
    }
}