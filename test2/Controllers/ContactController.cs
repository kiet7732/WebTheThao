using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class ContactController : Controller
    {
        WebBanHangEntities Data = new WebBanHangEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Chat chat)
        {
            if (Session["idUser"] == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập";
                return RedirectToAction("Index");
            }

            //if (string.IsNullOrEmpty(chat.HaiLong) || string.IsNullOrEmpty(chat.VanDe) || string.IsNullOrEmpty(chat.NoiDung))
            //{
            //    return View();
            //}

            chat.IdUser = Session["idUser"].ToString();
            chat.NgayGui = DateTime.Now;
            chat.Status = "Mới";

            Data.Chat.Add(chat);
            Data.SaveChanges();

            TempData["Message"] = "Gui thành công";
            return View();
        }
    }
}