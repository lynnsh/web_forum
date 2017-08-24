using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebForum.Models;

namespace WebForum.Controllers
{
    public class SearchController : Controller
    {
        private ForumEntities db = new ForumEntities();

        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        // GET: Search/Search
        public ActionResult Search(string keyword)
        {
            if (keyword == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var list = new List<Thread>();
            foreach(var item in db.Threads.Include("Tags")) {
                if(item.thread_title.Contains(keyword) || item.thread_text.Contains(keyword))
                    list.Add(item);
                else 
                {
                    foreach(var tag in item.Tags)
                        if(tag.tag_name.Contains(keyword))
                            list.Add(item);
                }
            }
            return View(list.Take(20));
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