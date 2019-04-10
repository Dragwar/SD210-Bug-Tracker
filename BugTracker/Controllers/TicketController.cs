using BugTracker.Models;
using BugTracker.MyHelpers.DB_Repositories.Ticket;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        public readonly ApplicationDbContext DbContext;
        public readonly TicketRepository TicketRepository;

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            TicketRepository = new TicketRepository(DbContext);
        }

        // GET: Ticket
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();



            return View();
        }

        // GET: Ticket/Details/{id}
        public ActionResult Details(Guid id)
        {
            return View();
        }

        //// GET: Ticket/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Ticket/Create
        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Ticket/Edit/{id}
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Ticket/Edit/{id}
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Ticket/Delete/{id}
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Ticket/Delete/{id}
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
