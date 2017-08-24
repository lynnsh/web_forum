using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebForum.Models;
using WebForum.Validators;

namespace WebForum.Controllers
{
    public class CommentsController : Controller
    {
        private ForumEntities db = new ForumEntities();

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Reply
        [Authorize]
        public ActionResult Reply(int? threadid, int? commentid)
        {
            if (threadid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Find(threadid);
            if (thread == null)
            {
                return HttpNotFound();
            }
            Comment newComment = new Comment();
            newComment.comment_date = DateTime.Now;
            newComment.thread_id = (int)threadid;
            if (commentid != null)
            {
                Comment comment = db.Comments.Find(commentid);
                if (comment == null)
                {
                    return HttpNotFound();
                }
                newComment.parent_comment_id = commentid;
            }
            return View(newComment);
        }

        [Authorize]
        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply([Bind(Include = "comment_date,comment_text,thread_id,parent_comment_id")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.username = User.Identity.Name;
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = comment.thread_id });
            }
            else
                ModelState.AddModelError("", "Invalid input");

            return View(comment);
        }

        // GET: Comments/Edit/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit([Bind(Include = "comment_id,comment_text,comment_date,username,thread_id,parent_comment_id")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = comment.thread_id });
            }
            else
                ModelState.AddModelError("", "Invalid input");
            return View(comment);
        }

        // GET: Comments/Delete/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [AdminAuthorize(Users = "admin")] 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            deleteComments(comment);
            db.SaveChanges();
            return RedirectToAction("Details", "Home", new { id = comment.thread_id });
        }

        private void deleteComments(Comment comment)
        {
            if (comment.Comments1.Count != 0)
            {
                var array = (from c in db.Comments
                             where c.parent_comment_id == comment.comment_id
                             select c);
                foreach (var comm in array)
                    deleteComments(comm);
            }
            db.Comments.Remove(comment);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
