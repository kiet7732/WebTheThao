using DoAn.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Areas.Admin.Controllers
{
    public class FeedBackController : BaseController
    {
        // GET: Admin/Comment
        WebBanHangEntities _data = new WebBanHangEntities();

        public class FeedBackDraw
        {
            WebBanHangEntities _data = new WebBanHangEntities();
            public FeedBackDraw()
            {
                _data = new WebBanHangEntities();
            }
            public int coutFeedBack()
            {
                return _data.Chat.Count();
            }
        }

        public ActionResult Index(string searchString, int page = 1)
        {
            var Chat = _data.Chat.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                Chat = Chat.Where(s => s.IdChat.Contains(searchString));
            }

            // Số lượng sản phẩm hiển thị trên mỗi trang
            int pageSize = 5;

            return View(Chat.OrderBy(s => s.IdChat).ToPagedList(page, pageSize));
        }


        public ActionResult Detaill(string id)
        {
            var Detail = _data.Chat.Find(id);

            if (Detail == null)
            {
                TempData["Message"] = "Không tồn tại.";
                return RedirectToAction("Index");
            }

            return View(Detail);
        }

        public ActionResult Done(string id)
        {
            var Done = _data.Chat.Find(id);

            if (Done != null)
            {
                Done.Status = "Đã Xem";
                TempData["Message"] = "Đã Xem";

                _data.SaveChanges();
                return RedirectToAction("Index");
            }

            TempData["Message"] = "Không tồn tại.";
            return RedirectToAction("Index");
        }
    }
}