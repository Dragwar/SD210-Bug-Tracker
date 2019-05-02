using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.InteropServices;
using BugTracker.Models;
using BugTracker.Models.Domain;

namespace BugTracker.MyHelpers.DB_Repositories
{
    public class TicketHistoryRepository
    {
        private readonly ApplicationDbContext DbContext;

        public TicketHistoryRepository(ApplicationDbContext dbContext) => DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public EntityState? GetTicketState(Guid id) => DbContext.ChangeTracker.Entries<Ticket>().FirstOrDefault(ticket => ticket.Entity.Id == id)?.State;

        public List<TicketHistory> GetTicketChanges(Ticket ticketToCheck, string userIdWhoMadeChanges, [Optional] List<string> avoidTheseMembers)
        {
            #region Null Checks
            if (string.IsNullOrWhiteSpace(userIdWhoMadeChanges) || !new UserRepository(DbContext).DoesUserExist(userIdWhoMadeChanges))
            {
                throw new ArgumentException("Not found with this id", nameof(userIdWhoMadeChanges));
            }

            bool canUserEditTicket = new TicketRepository(DbContext).CanUserEditTicket(userIdWhoMadeChanges, ticketToCheck.Id);

            if (!canUserEditTicket)
            {
                throw new Exception("User doesn't have the proper permissions to edit the ticket");
            }
            #endregion

            avoidTheseMembers = (avoidTheseMembers?.Count ?? 0) > 0 ? avoidTheseMembers : new List<string>()
            {
                nameof(ticketToCheck.TicketHistories),
                nameof(ticketToCheck.Attachments),
                nameof(ticketToCheck.Comments),
                nameof(ticketToCheck.DateCreated),
                nameof(ticketToCheck.DateUpdated),
            };

            DbEntityEntry<Ticket> foundTicketEntry = DbContext.ChangeTracker
                .Entries<Ticket>()
                .First(ticket => ticket.Entity.Id == ticketToCheck.Id);

            List<TicketHistory> ticketHistories = new List<TicketHistory>();

            if (foundTicketEntry.State == EntityState.Modified)
            {
                foreach (string memberName in foundTicketEntry.OriginalValues.PropertyNames)
                {
                    if (avoidTheseMembers.Contains(memberName))
                    {
                        continue;
                    }

                    string oldValue = foundTicketEntry.OriginalValues[memberName]?.ToString();
                    string newValue = foundTicketEntry.CurrentValues[memberName]?.ToString();


                    if (oldValue != newValue)
                    {
                        string name = memberName;

                        // Remove Ids and display name/title of changed member
                        if (name.Contains("Id"))
                        {
                            switch (name)
                            {
                                case nameof(ticketToCheck.ProjectId):
                                    ProjectRepository projectRepo = new ProjectRepository(DbContext);

                                    oldValue = projectRepo
                                        .GetProject(Guid.Parse(oldValue))?
                                        .Name ?? throw new Exception("Project not found");

                                    newValue = projectRepo
                                        .GetProject(Guid.Parse(newValue))?
                                        .Name ?? throw new Exception("Project not found");
                                    break;

                                case nameof(ticketToCheck.AssignedUserId):
                                    UserRepository userRepo = new UserRepository(DbContext);

                                    // Handle when user had no assigned user to start having one
                                    const string message = "User not found";

                                    // Able to assign user from ticket (from null to assigning a new user)
                                    try
                                    {
                                        oldValue = userRepo
                                            .GetUserById(oldValue)?
                                            .Email ?? throw new Exception(message);
                                    }
                                    catch (Exception e)
                                    {
                                        if (e.Message == message)
                                        {
                                            oldValue = "No User";
                                        }
                                    }

                                    // Able to unassign user from ticket (and leave it null)
                                    try
                                    {
                                        newValue = userRepo
                                            .GetUserById(newValue)?
                                            .Email ?? throw new Exception(message);
                                    }
                                    catch (Exception e)
                                    {
                                        if (e.Message == message)
                                        {
                                            newValue = "No User";
                                        }
                                    }
                                    break;

                                case nameof(ticketToCheck.PriorityId):
                                    oldValue = DbContext.TicketPriorities
                                        .First(ticketPriority => ticketPriority.Id.ToString() == oldValue)?
                                        .PriorityString ?? throw new Exception("Ticket Priority not found");

                                    newValue = DbContext.TicketPriorities
                                        .First(ticketPriority => ticketPriority.Id.ToString() == newValue)?
                                        .PriorityString ?? throw new Exception("Ticket Priority not found");
                                    break;

                                case nameof(ticketToCheck.TypeId):
                                    oldValue = DbContext.TicketTypes
                                        .First(ticketType => ticketType.Id.ToString() == oldValue)?
                                        .TypeString ?? throw new Exception("Ticket Type not found");

                                    newValue = DbContext.TicketTypes
                                        .First(ticketType => ticketType.Id.ToString() == newValue)?
                                        .TypeString ?? throw new Exception("Ticket Type not found");
                                    break;

                                case nameof(ticketToCheck.StatusId):
                                    oldValue = DbContext.TicketStatuses
                                        .First(ticketStatus => ticketStatus.Id.ToString() == oldValue)?
                                        .StatusString ?? throw new Exception("Ticket Status not found");

                                    newValue = DbContext.TicketStatuses
                                        .First(ticketStatus => ticketStatus.Id.ToString() == newValue)?
                                        .StatusString ?? throw new Exception("Ticket Status not found");
                                    break;
                            }
                            name = memberName.Replace("Id", "");
                        }

                        ticketHistories.Add(CreateNewTicketHistory(false, userIdWhoMadeChanges, ticketToCheck.Id, name, oldValue, newValue));
                    }
                }
            }
            return ticketHistories;
        }

        public TicketHistory CreateNewTicketHistory(bool saveChanges, string userIdWhoMadeChange, Guid ticketId, string property, string oldValue, string newValue)
        {
            if (ticketId == null || ticketId == Guid.Empty ||
                string.IsNullOrWhiteSpace(userIdWhoMadeChange) ||
                string.IsNullOrWhiteSpace(property) ||
                string.IsNullOrWhiteSpace(oldValue) ||
                string.IsNullOrWhiteSpace(newValue))
            {
                throw new ArgumentNullException();
            }

            TicketHistory newTicketHistory = new TicketHistory()
            {
                Property = property,
                OldValue = oldValue,
                NewValue = newValue,
                TicketId = ticketId,
                UserId = userIdWhoMadeChange,
            };

            if (saveChanges)
            {
                DbContext.TicketHistories.Add(newTicketHistory);
                DbContext.SaveChanges();
            }

            return newTicketHistory;
        }
    }
}