using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebForum.Models;
using System.Web.Security;

namespace WebForum.Controllers
{
    public class AccountController : Controller
    {
        private ForumEntities db = new ForumEntities();


        // GET: Account/Details
        [Authorize]
        public ActionResult Details()
        {
            User user = db.Users.Find(User.Identity.Name);
            return View(user);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "username,password,first_name,last_name,email,interests,favourite_quote")] User user)
        {
            if (ModelState.IsValid && user.username != null)
            {
                var query = (from u in db.Users
                             where u.username.Equals(user.username)
                             select u).FirstOrDefault();
                if (query != null)
                {
                    ModelState.AddModelError("", "This username already exists");
                    return View(user);

                }
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index", "Home", null);
            }
            else
                ModelState.AddModelError("", "Invalid input");
            return View(user);
        }

        // GET: Account/Login
        public ActionResult Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // POST: Account/Login
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var query = (from u in db.Users
                             where u.username.Equals(username) && u.password.Equals(password)
                             select u).FirstOrDefault();
                if (query == null)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    ViewBag.ReturnUrl = ReturnUrl;
                    return View();

                }

                FormsAuthentication.RedirectFromLoginPage(username, false);
            }

            return View();
        }

        // GET: Account/Edit/
        [Authorize]
        public ActionResult Edit()
        {
            User user = db.Users.Find(User.Identity.Name);
            return View(user);
        }

        // POST: Account/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "password,first_name,last_name,email,interests,favourite_quote")] User user)
        {
            user.username = User.Identity.Name;
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            else
                ModelState.AddModelError("", "Invalid input");
            return View(user);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", null);
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
