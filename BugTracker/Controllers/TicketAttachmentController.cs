using System;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters.Authorize;
using BugTracker.Models.ViewModels.TicketAttachment;
using BugTracker.MyHelpers;
using BugTracker.MyHelpers.DB_Repositories;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [BugTrackerAuthorize]
    public class TicketAttachmentController : Controller
    {
        private readonly ApplicationDbContext DbContext;
        private readonly TicketRepository TicketRepository;
        private readonly UserRepository UserRepository;
        private readonly TicketCommentRepository TicketCommentRepository;
        private readonly TicketAttachmentRepository TicketAttachmentRepository;

        public TicketAttachmentController()
        {
            DbContext = new ApplicationDbContext();
            TicketRepository = new TicketRepository(DbContext);
            UserRepository = new UserRepository(DbContext);
            TicketCommentRepository = new TicketCommentRepository(DbContext);
            TicketAttachmentRepository = new TicketAttachmentRepository(DbContext);
        }

        // GET: TicketAttachment
        public ActionResult Index(Guid? ticketId)
        {
            if (!ticketId.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Index), "Ticket");
            }
            else if (TicketRepository.DoesTicketExist(ticketId.Value))
            {
                return RedirectToAction(nameof(TicketController.Details), "Ticket", new { id = ticketId });
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        // GET: TicketAttachment/Create
        public ActionResult Create(Guid? ticketId)
        {
            if (!ticketId.HasValue || !TicketRepository.DoesTicketExist(ticketId.Value))
            {
                return RedirectToAction(nameof(Index));
            }
            Ticket foundTicket = TicketRepository.GetTicket(ticketId.Value) ?? throw new Exception("This shouldn't happen");
            string userId = User.Identity.GetUserId();
            if (TicketRepository.CanUserEditTicket(userId, foundTicket.Id))
            {
                return View(TicketAttachmentCreateViewModel.CreateNewViewModel(foundTicket));
            }
            else
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"You don't have the appropriate permissions to add a attachment to this ticket ({foundTicket.Title})" });
            }
        }

        // POST: TicketAttachment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TicketAttachmentCreateViewModel formData)
        {
            if (formData == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string userId = User.Identity.GetUserId();
                if (!UserRepository.DoesUserExist(userId))
                {
                    throw new Exception("User Not Found");
                }
                if (!TicketRepository.CanUserEditTicket(userId, formData.TicketId))
                {
                    return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"You don't have the appropriate permissions to add a attachment to this ticket ({formData.TicketTitle})" });
                }
                if (formData.Media == null)
                {
                    ModelState.AddModelError(nameof(TicketAttachmentCreateViewModel.Media), "You must attach/upload a file");
                    return View(formData);
                }

                FileSystemRepository fileSystemRepository = new FileSystemRepository(Server, null);

                TicketAttachment newTicketAttachment = new TicketAttachment()
                {
                    Description = formData.Description,
                    UserId = userId,
                    TicketId = formData.TicketId,
                };

                (bool hasSuccessfullySaved, string filePath, string fileUrl, string resultMessage) = fileSystemRepository.SaveFile(formData.Media);

                if (!hasSuccessfullySaved)
                {
                    throw new Exception($"File wasn't saved\n\tFileSystemRepository - Message: {resultMessage}");
                }

                newTicketAttachment.FilePath = filePath;
                newTicketAttachment.FileUrl = Url.Content(fileUrl);

                DbContext.TicketAttachments.Add(newTicketAttachment);
                int savedEntities = DbContext.SaveChanges();

                if (savedEntities <= 0)
                {
                    throw new Exception($"Database wasn't saved\n\tFileSystemRepository - Message: {resultMessage}");
                }

                return RedirectToAction(nameof(Index), new { ticketId = formData.TicketId });
            }
            catch
            {
                if (formData?.TicketId != null && !TicketRepository.DoesTicketExist(formData.TicketId))
                {
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Create), new { ticketId = formData.TicketId });
            }
        }

        // GET: TicketAttachment/Delete/{id}
        public ActionResult Delete(Guid? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }
            string userId = User.Identity.GetUserId();
            TicketAttachment foundTicketAttachment = TicketAttachmentRepository.GetTicketAttachment(id.Value);
            if (foundTicketAttachment == null)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"Ticket Attachment wasn't deleted (Not Found)" });
            }
            else if (!TicketRepository.CanUserEditTicket(userId, foundTicketAttachment.TicketId))
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"You don't have the appropriate permissions to add a attachment to this ticket ({foundTicketAttachment.Ticket.Title})" });
            }
            return View(TicketAttachmentDeleteViewModel.CreateNewViewModel(foundTicketAttachment));
        }

        // POST: TicketAttachment/Delete/{id}
        [HttpPost]
        public ActionResult Delete(Guid? id, TicketAttachmentDeleteViewModel formData)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string userId = User.Identity.GetUserId();
                TicketAttachment foundTicketAttachment = TicketAttachmentRepository.GetTicketAttachment(formData.Id);
                if (foundTicketAttachment == null)
                {
                    return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"Ticket Attachment wasn't deleted (Not Found)" });
                }
                else if (!TicketRepository.CanUserEditTicket(userId, foundTicketAttachment.TicketId))
                {
                    return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"You don't have the appropriate permissions to add a attachment to this ticket ({foundTicketAttachment.Ticket.Title})" });
                }
                DbContext.TicketAttachments.Remove(foundTicketAttachment);
                int numberOfSavedEntities = DbContext.SaveChanges();
                if (numberOfSavedEntities <= 0)
                {
                    return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"Ticket Attachment wasn't deleted (Database Error)" });
                }
                return RedirectToAction(nameof(Index), new { ticketId = formData.TicketId });
            }
            catch
            {
                return RedirectToAction(nameof(Delete), new { id = formData.Id });
            }
        }

    }
}
