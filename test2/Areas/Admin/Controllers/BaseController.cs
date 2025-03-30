using DoAn.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAn.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        // GET: Admin/Base
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["adminLogin"] == null)
            {
                // Redirect to Admin login if not authenticated
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Logins", action = "Index"})
                );
            }
            base.OnActionExecuting(filterContext);
        }
    }
}