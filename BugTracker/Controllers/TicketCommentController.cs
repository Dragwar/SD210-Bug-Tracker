using System;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters.Authorize;
using BugTracker.Models.ViewModels.TicketComment;
using BugTracker.MyHelpers.DB_Repositories;
using BugTracker.MyHelpers.DB_Repositories.Ticket;
using BugTracker.MyHelpers.DB_Repositories.TicketComment;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [BugTrackerAuthorize]
    public class TicketCommentController : Controller
    {
        private readonly ApplicationDbContext DbContext;
        private readonly TicketRepository TicketRepository;
        private readonly UserRoleRepository UserRoleRepository;
        private readonly UserRepository UserRepository;
        private readonly TicketCommentRepository TicketCommentRepository;

        public TicketCommentController()
        {
            DbContext = new ApplicationDbContext();
            TicketRepository = new TicketRepository(DbContext);
            UserRepository = new UserRepository(DbContext);
            UserRoleRepository = new UserRoleRepository(DbContext);
            TicketCommentRepository = new TicketCommentRepository(DbContext);
        }

        // GET: TicketComment
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

        [NonAction]
        private ActionResult ReturnCreateEditResult((bool doesUserExist, bool doesTicketExist, bool canAddComment) bools, ITicketCommentCreateEditViewModel model)
        {
            if (bools.doesUserExist && bools.doesTicketExist && bools.canAddComment)
            {
                return View(model);
            }
            else if (!bools.canAddComment)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = $"You don't have the appropriate permissions to add a comment to this ticket ({model.TicketName})" });
            }
            else if (bools.doesTicketExist)
            {
                return RedirectToAction(nameof(Index), new { ticketId = model.TicketId });
            }
            else
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "Something went wrong" });
            }
        }

        // GET: TicketComment/Create
        public ActionResult Create(Guid? ticketId)
        {
            if (!ticketId.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Index), "Ticket");
            }

            string userId = User.Identity.GetUserId();
            bool doesUserExist = UserRepository.DoesUserExist(userId);

            Ticket foundTicket = TicketRepository.GetTicket(ticketId.Value);
            bool doesTicketExist = foundTicket != null ? true : false;

            bool canAddComment = TicketRepository.CanUserEditTicket(userId, ticketId.Value);


            return ReturnCreateEditResult((doesUserExist, doesTicketExist, canAddComment), TicketCommentCreateViewModel.CreateNewViewModel(foundTicket, userId));
        }

        // POST: TicketComment/Create
        [HttpPost]
        public ActionResult Create(TicketCommentCreateViewModel formData)
        {
            if (formData == null)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "Something went wrong" });
            }

            try
            {
                Ticket foundTicket = TicketRepository.GetTicket(formData.TicketId) ?? throw new ArgumentException("Ticket not found");

                TicketComments newTicketComment = new TicketComments()
                {
                    Comment = formData.Comment,
                    UserId = formData.UserId,
                };

                foundTicket.Comments.Add(newTicketComment);
                DbContext.SaveChanges();

                return RedirectToAction(nameof(Index), new { ticketId = formData.TicketId });
            }
            catch
            {
                return RedirectToAction(nameof(Create), new { ticketId = formData.TicketId });
            }
        }

        // GET: TicketComment/Edit/{id}
        public ActionResult Edit(Guid? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Index), "Ticket");
            }

            string userId = User.Identity.GetUserId();
            bool doesUserExist = UserRepository.DoesUserExist(userId);

            TicketComments foundTicketComment = TicketCommentRepository.GetTicketComment(id.Value);

            if (foundTicketComment == null)
            {
                return RedirectToAction(nameof(TicketController.Index), "Ticket");
            }

            bool doesTicketExist = TicketRepository.DoesTicketExist(foundTicketComment.TicketId);
            bool canAddComment = TicketRepository.CanUserEditTicket(userId, foundTicketComment.TicketId);

            return ReturnCreateEditResult((doesUserExist, doesTicketExist, canAddComment), TicketCommentEditViewModel.CreateNewViewModel(foundTicketComment));
        }

        // POST: TicketComment/Edit/{id}
        [HttpPost]
        public ActionResult Edit(TicketCommentEditViewModel formData)
        {
            if (formData == null)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "Something went wrong" });
            }

            try
            {
                TicketComments foundTicketComment = TicketCommentRepository.GetTicketComment(formData.Id) ?? throw new Exception("Ticket Comment Not Found");
                foundTicketComment.Comment = formData.Comment;
                DbContext.SaveChanges();
                return RedirectToAction(nameof(Index), new { ticketId = formData.TicketId });
            }
            catch
            {
                return RedirectToAction(nameof(Edit), new { id = formData.Id });
            }
        }

        // GET: TicketComment/Delete/{id}
        public ActionResult Delete(Guid? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "Something went wrong (Comment Wasn't deleted)" });
            }

            TicketComments foundTicketComment = TicketCommentRepository.GetTicketComment(id.Value);

            if (foundTicketComment == null)
            {
                return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "Something went wrong (Comment Wasn't deleted)" });
            }

            return View(TicketCommentDeleteViewModel.CreateNewViewModel(foundTicketComment));
        }

        // POST: TicketComment/Delete/{id}
        [HttpPost]
        public ActionResult Delete(TicketCommentDeleteViewModel formData)
        {
            try
            {
                bool hasBeenDeleted = TicketCommentRepository.DeleteTicketComment(formData.Id);

                if (!hasBeenDeleted)
                {
                    return RedirectToAction(nameof(HomeController.UnauthorizedRequest), "Home", new { error = "Something went wrong (Comment Wasn't deleted)" });
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
