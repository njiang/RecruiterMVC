using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestHireChannelMVC.Models;

namespace TestHireChannelMVC.Controllers
{ 
    public class JobController : Controller
    {
        //private JobContext db = new JobContext();

        //
        // GET: /Job/

        public ViewResult Index()
        {
            return View(db.Users.ToList());
        }

        //
        // GET: /Job/Details/5

        public ViewResult Details(Guid id)
        {
            Job job = db.Users.Find(id);
            return View(job);
        }

        //
        // GET: /Job/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Job/Create

        [HttpPost]
        public ActionResult Create(Job job)
        {
            if (ModelState.IsValid)
            {
                job.JobID = Guid.NewGuid();
                db.Users.Add(job);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(job);
        }
        
        //
        // GET: /Job/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            Job job = db.Users.Find(id);
            return View(job);
        }

        //
        // POST: /Job/Edit/5

        [HttpPost]
        public ActionResult Edit(Job job)
        {
            if (ModelState.IsValid)
            {
                db.Entry(job).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(job);
        }

        //
        // GET: /Job/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Job job = db.Users.Find(id);
            return View(job);
        }

        //
        // POST: /Job/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Job job = db.Users.Find(id);
            db.Users.Remove(job);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}