using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static DoAn.Areas.Admin.Controllers.FeedBackController;
using static DoAn.Areas.Admin.Controllers.OrderController;
using static DoAn.Areas.Admin.Controllers.ProductController;
using static DoAn.Areas.Admin.Controllers.UsersController;

namespace DoAn.Areas.Admin.Controllers
{
    public class HomesController : BaseController
    {
        // GET: Admin/Homes   Base

        //    /Admin/Homes/Index
        public ActionResult Index()
        {
            ViewBag.coutProduct = new ProductDraw().coutProduct();
            ViewBag.coutUser = new UserDraw().coutUser();
            ViewBag.coutOrder = new OrderDraw().coutOrder();
            ViewBag.coutFeedBack = new FeedBackDraw().coutFeedBack();
            return View();
        }

        [HttpPost]
        public JsonResult ClearMessage()
        {
            TempData["Message"] = null;
            return Json(new { status = true });
        }
    }
}