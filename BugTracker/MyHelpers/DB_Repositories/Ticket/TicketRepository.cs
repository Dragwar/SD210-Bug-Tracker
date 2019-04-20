using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BugTracker.Models;

namespace BugTracker.MyHelpers.DB_Repositories.Ticket
{
    [NotMapped]
    public class TicketRepository
    {
        private ApplicationDbContext DBContext { get; }

        public TicketRepository(ApplicationDbContext dBContext) => DBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));

        public Models.Domain.Ticket GetTicket(Guid id) => DBContext.Tickets.FirstOrDefault(ticket => ticket.Id == id);
        public bool DoesTicketExist(Guid id) => DBContext.Tickets.Any(ticket => ticket.Id == id);
        public bool CanUserViewTicket(string userId, Guid ticketId)
        {
            if (ticketId == null || ticketId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DBContext).GetIsUserInRoleDictionary(userId);

            Models.Domain.Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

            bool isUserAssignedToProject = foundTicket.Project.Users.Any(user => user.Id == userId);

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (foundTicket.Author != null && isInRole[UserRolesEnum.Submitter] && foundTicket.Author.Id == userId)
            {
                return true;
            }
            else if (foundTicket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && foundTicket.AssignedUser.Id == userId)
            {
                return true;
            }
            else if (isUserAssignedToProject)
            {
                return true;
            }

            return false;
        }
        #region CanUserViewTicket (overloads)
        public bool CanUserViewTicket(string userId, Models.Domain.Ticket ticket)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DBContext).GetIsUserInRoleDictionary(userId);

            bool isUserAssignedToProject = ticket.Project.Users.Any(user => user.Id == userId);

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (ticket.Author != null && isInRole[UserRolesEnum.Submitter] && ticket.Author.Id == userId)
            {
                return true;
            }
            else if (ticket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && ticket.AssignedUser.Id == userId)
            {
                return true;
            }
            else if (isUserAssignedToProject)
            {
                return true;
            }

            return false;
        }
        #endregion

        public bool CanUserEditTicket(string userId, Guid ticketId)
        {
            if (ticketId == null || ticketId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DBContext).GetIsUserInRoleDictionary(userId);

            Models.Domain.Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (foundTicket.Author != null && isInRole[UserRolesEnum.Submitter] && foundTicket.Author.Id == userId)
            {
                return true;
            }
            else if (foundTicket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && foundTicket.AssignedUser.Id == userId)
            {
                return true;
            }

            return false;
        }
        #region CanUserViewTicket (overloads)
        public bool CanUserEditTicket(string userId, Models.Domain.Ticket ticket)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DBContext).GetIsUserInRoleDictionary(userId);

            if (isInRole[UserRolesEnum.Admin] || isInRole[UserRolesEnum.ProjectManager])
            {
                return true;
            }
            else if (ticket.Author != null && isInRole[UserRolesEnum.Submitter] && ticket.Author.Id == userId)
            {
                return true;
            }
            else if (ticket.AssignedUser != null && isInRole[UserRolesEnum.Developer] && ticket.AssignedUser.Id == userId)
            {
                return true;
            }

            return false;
        }
        #endregion

        public IQueryable<Models.Domain.Ticket> GetUserCreatedTickets(string userId) => GetAllTickets()
            .Where(ticket => ticket.Author.Id == userId)
            .AsQueryable();
        public IQueryable<Models.Domain.Ticket> GetUserAssignedTickets(string userId) => GetAllTickets()
            .Where(ticket => ticket.AssignedUser != null && ticket.AssignedUser.Id == userId)
            .AsQueryable();

        //! Easier way than above (using the List that exists on the ApplicationUser instead of directly querying the all ticket List)
        //public IQueryable<Models.Domain.Ticket> GetUserCreatedTickets(string userId) => DBContext.Users
        //    .FirstOrDefault(user => user.Id == userId)?
        //    .CreatedTickets
        //    .AsQueryable() ?? new List<Models.Domain.Ticket>().AsQueryable();
        //public IQueryable<Models.Domain.Ticket> GetUserAssignedTickets(string userId) => DBContext.Users
        //    .FirstOrDefault(user => user.Id == userId)?
        //    .AssignedTickets
        //    .AsQueryable() ?? new List<Models.Domain.Ticket>().AsQueryable();

        public IQueryable<Models.Domain.Ticket> GetAllTickets() => DBContext.Tickets.AsQueryable();
    }
}