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
    public class HomeController : Controller
    {
        private ForumEntities db = new ForumEntities();

        // GET: Home
        public ActionResult Index()
        {
            var threads = (from t in db.Threads.Include("Tags")
                          orderby t.thread_date descending
                          select t).Take<Thread>(20);
            return View(threads);
        }

        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Include(x => x.Tags).SingleOrDefault(x => x.thread_id == id);
            if (thread == null)
            {
                return HttpNotFound();
            }
            thread.thread_views++;
            db.SaveChanges();
            var query = (from c in db.Comments
                         where c.parent_comment_id == null && c.thread_id == id
                         select c).ToList();
            ViewBag.List = query;
            return View(thread);
        }

        // GET: Home/Create
        [Authorize]
        public ActionResult Create()
        {
            var query = (from tag in db.Tags select tag).ToList();
            ViewBag.Tags = query;
            return View();
        }

        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "thread_title,thread_text")] Thread thread,
                                    string[] tags, string tag)
        {
            thread.thread_date = DateTime.Now;
            thread.thread_views = 0;
            thread.thread_upvotes = 0;
            thread.username = User.Identity.Name;

            addTags(tags, tag, thread);
           
            if (ModelState.IsValid)
            {
                db.Threads.Add(thread);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
	            ModelState.AddModelError("", "Invalid input");

            return View(thread);
        }

        // GET: Home/Edit/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Find(id);
            if (thread == null)
            {
                return HttpNotFound();
            }
            return View(thread);
        }

        // POST: Home/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit([Bind(Include = "thread_id,username,thread_date,thread_views,thread_upvotes,thread_title,thread_text")] Thread thread)
        {
            if (ModelState.IsValid)
            {
                db.Entry(thread).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = thread.thread_id });
            }
            else
                ModelState.AddModelError("", "Invalid input");
            return View(thread);
        }

        // GET: Home/Delete/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Find(id);
            if (thread == null)
            {
                return HttpNotFound();
            }
            return View(thread);
        }

        // POST: Home/Delete/5
        [AdminAuthorize(Users = "admin")] 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Thread thread = db.Threads.Find(id);
            db.Threads.Remove(thread);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Home/Vote/
        [Authorize]
        public ActionResult Vote(int? threadid, int? vote)
        {
            if (threadid == null || vote == null || (vote != 0 && vote != 1))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Thread thread = db.Threads.Include(x => x.Tags).SingleOrDefault(x => x.thread_id == threadid);
            if (thread == null)
            {
                return HttpNotFound();
            }
            if (vote == 0)
                thread.thread_upvotes++;
            else
                thread.thread_upvotes--;
            db.SaveChanges();
            var query = (from c in db.Comments
                         where c.parent_comment_id == null && c.thread_id == thread.thread_id
                         select c).ToList();
            ViewBag.List = query;
            return View("Details", thread);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void addTags(string[] tags, string tag, Thread thread)
        {
            Tag threadTag = null;
            if (tags != null && tags.Length > 0)
            {
                List<Tag> list = convert(tags);
                foreach (var item in list)
                    thread.Tags.Add(item);
                //find tag in list of current thread tags
                threadTag = thread.Tags.FirstOrDefault(item => item.tag_name.Equals(tag));
            }

            if (tag != null && !tag.Trim().Equals(""))
            {
                tag = tag.Trim();
                if (threadTag == null)
                {
                    //find tag in list of all tags
                    Tag listTag = (from t in db.Tags where t.tag_name.Equals(tag) select t).FirstOrDefault();
                    if (listTag == null) 
                        thread.Tags.Add(new Tag { tag_name = tag });
                    else
                        thread.Tags.Add(listTag);
                }
            }
        }

        private List<Tag> convert(string[] tags)
        {
            return (from tag in db.Tags
                         where tags.Contains(tag.tag_name)
                         select tag).ToList(); 
        }


    }
}
