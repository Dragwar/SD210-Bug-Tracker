using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels.Ticket;

namespace BugTracker.MyHelpers.DB_Repositories
{
    [NotMapped]
    public class TicketRepository
    {
        private readonly ApplicationDbContext DbContext;

        public TicketRepository(ApplicationDbContext dBContext) => DbContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));

        /// <summary>This would get all tickets first, then this applies parameters</summary>
        public List<TSelectType> GetTickets<TSelectType>(Func<Ticket, bool> where, Func<Ticket, TSelectType> select) => GetAllTickets()
            .ToList()
            .Where(ticket => where(ticket))
            .Select(ticket => select(ticket))
            .ToList();

        /// <summary>This would get all tickets first, then this applies <paramref name="where"/> parameter</summary>
        public List<Ticket> GetTickets(Func<Ticket, bool> where) => GetAllTickets()
            .ToList()
            .Where(ticket => where(ticket))
            .ToList();

        public Ticket GetTicket(Guid id) => DbContext.Tickets.FirstOrDefault(ticket => ticket.Id == id);
        public bool DoesTicketExist(Guid id) => DbContext.Tickets.Any(ticket => ticket.Id == id);
        public bool CanUserViewTicket(string userId, Guid ticketId)
        {
            if (ticketId == null || ticketId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException();
            }

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

            Ticket foundTicket = GetTicket(ticketId) ?? throw new Exception("Ticket not found");

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

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

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

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

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

            IReadOnlyDictionary<UserRolesEnum, bool> isInRole = new UserRoleRepository(DbContext).GetIsUserInRoleDictionary(userId);

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


        public Ticket CreateNewTicket(TicketCreateViewModel model, bool saveChanges)
        {
            if (DbContext == null ||
                model == null ||
                string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                !model.Priority.HasValue ||
                !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }

            Project foundProject = new ProjectRepository(DbContext).GetProject(model.ProjectId) ?? throw new Exception($"project not found {model.ProjectId}");
            ApplicationUser foundAuthor = new UserRepository(DbContext).GetUserById(model.AuthorId) ?? throw new Exception($"user not found {model.AuthorId}");

            Ticket newTicket = new Ticket()
            {
                Title = model.Title,
                Description = model.Description,
                TypeId = (int)model.Type.Value,
                PriorityId = (int)model.Priority.Value,
                StatusId = (int?)model?.Status ?? (int)TicketStatusesEnum.Open,
            };

            foundProject.Tickets.Add(newTicket);
            foundAuthor.CreatedTickets.Add(newTicket);

            if (saveChanges)
            {
                DbContext.SaveChanges();
            }

            return newTicket;
        }


        /// <summary>
        /// PLEASE FIND A BETTER WAY
        /// </summary>
        public Ticket EditExistingTicket(Ticket ticket, TicketEditViewModel model, string currentUserId, (bool forceChecks, bool saveDatabase) config)
        {
            if (ticket == null || model == null ||
                string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                !model.Priority.HasValue ||
                !model.Type.HasValue)
            {
                throw new ArgumentException("bad data");
            }
            bool wasEdited = !ticket.Equals(model);
            List<TicketHistory> ticketHistories = new List<TicketHistory>();

            TicketHistoryRepository ticketHistoryRepository = new TicketHistoryRepository(DbContext);

            if (wasEdited || config.forceChecks)
            {
                if (ticket.Title != model.Title)
                {
                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.Title),
                        ticket.Title,
                        model.Title));

                    ticket.Title = model.Title;
                }

                if (ticket.Description != model.Description)
                {
                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.Description),
                        ticket.Description,
                        model.Description));

                    ticket.Description = model.Description;
                }

                if (ticket.PriorityId != (int)model.Priority)
                {
                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.Priority),
                        ticket.Priority.PriorityString,
                        model.Priority.ToString()));

                    ticket.PriorityId = (int)model.Priority;
                }

                if (ticket.StatusId != ((int?)model?.Status ?? (int)TicketStatusesEnum.Open))
                {
                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.Status),
                        ticket.Status.StatusString,
                        model.Status.ToString()));

                    ticket.StatusId = (int?)model?.Status ?? (int)TicketStatusesEnum.Open;
                }

                if (ticket.TypeId != (int)model.Type)
                {
                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.Type),
                        ticket.Type.TypeString,
                        model.Type.ToString()));

                    ticket.TypeId = (int)model.Type;
                }

                if (ticket.ProjectId != model.ProjectId)
                {
                    Project foundProject = new ProjectRepository(DbContext)
                        .GetProject(model.ProjectId) ?? throw new Exception(nameof(foundProject) + " wasn't found");

                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.Project),
                        ticket.Project.Name,
                        foundProject.Name));

                    ticket.ProjectId = model.ProjectId;
                }

                if (ticket.AssignedUserId != model.DeveloperId)
                {
                    ApplicationUser foundDev = new UserRepository(DbContext)
                        .GetUserById(model.DeveloperId) ?? throw new Exception(nameof(foundDev) + " wasn't found");

                    ticketHistories.Add(ticketHistoryRepository.CreateNewTicketHistory(
                        false,
                        currentUserId,
                        ticket.Id,
                        nameof(ticket.AssignedUser),
                        ticket.AssignedUser.Email,
                        foundDev.Email));

                    ticket.AssignedUserId = model.DeveloperId;
                }


                foreach (TicketHistory ticketHistory in ticketHistories)
                {
                    DbContext.TicketHistories.Add(ticketHistory);
                }

                ticket.DateUpdated = DateTime.Now;
            }

            if (config.saveDatabase)
            {
                DbContext.SaveChanges();
            }

            return ticket;
        }

        public IQueryable<Ticket> GetAllTickets() => DbContext.Tickets.AsQueryable();
    }
}