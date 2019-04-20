using System;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Filters.Authorize;
using BugTracker.MyHelpers.DB_Repositories;

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

        // GET: TicketAttachment/Details/{id}
        public ActionResult Details(Guid? id) => throw new NotImplementedException();

        // GET: TicketAttachment/Create
        public ActionResult Create() => throw new NotImplementedException();

        // POST: TicketAttachment/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            throw new NotImplementedException();
            //try
            //{
            //    // TODO: Add insert logic here

            //    return RedirectToAction("Index");
            //}
            //catch
            //{
            //    return View();
            //}
        }

        // GET: TicketAttachment/Edit/{id}
        public ActionResult Edit(Guid? id) => throw new NotImplementedException();

        // POST: TicketAttachment/Edit/{id}
        [HttpPost]
        public ActionResult Edit(Guid? id, FormCollection collection)
        {
            throw new NotImplementedException();
            //try
            //{
            //    // TODO: Add update logic here

            //    return RedirectToAction("Index");
            //}
            //catch
            //{
            //    return View();
            //}
        }

        // GET: TicketAttachment/Delete/{id}
        public ActionResult Delete(Guid? id) => throw new NotImplementedException();

        // POST: TicketAttachment/Delete/{id}
        [HttpPost]
        public ActionResult Delete(Guid? id, FormCollection collection)
        {
            throw new NotImplementedException();
            //try
            //{
            //    // TODO: Add delete logic here

            //    return RedirectToAction("Index");
            //}
            //catch
            //{
            //    return View();
            //}
        }
    }
}
