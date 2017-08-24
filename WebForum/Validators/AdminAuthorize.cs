using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebForum.Validators
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //check if the user is authenticated: if s/he is, it means that the request is 
            // unauthorized because of their User name
            if (filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                //set a name value pair in TempData
                filterContext.Controller.TempData.Add("RedirectReason", "Unauthorized");
            }
            base.HandleUnauthorizedRequest(filterContext);
        }
    }

}