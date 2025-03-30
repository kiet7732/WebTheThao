using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using DoAn.Controllers;
using DoAn.Models;
using Npgsql;
using PagedList;

namespace test2.Controllers
{
    public class HomeController : Controller
    {
        WebBanHangEntities Data = new WebBanHangEntities();
        public ActionResult HomeShop()
        {
            
            return View(); 
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Shop()
        {
            return View();
        }


        public ActionResult PayCart()
        {
            return View();
        }

        public ActionResult FooterPartial()
        {
            return PartialView();
        }

        public ActionResult NavPartial()
        {
            

            return PartialView();
        }

        public ActionResult ProfileUser()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult ClearMessage()
        {
            TempData["Message"] = null;  
            return Json(new { status = true });
        }

        public ActionResult SearchResults(string query, int? page)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View();
            }

            int pageNumber = (page ?? 1);  // Mặc định trang 1 nếu không có tham số page
            int pageSize = 12; // Bạn có thể điều chỉnh số sản phẩm trên mỗi trang

            var results = Data.SanPham
                              .Where(s => s.TenSanPham.ToLower().Contains(query.ToLower()))  // Tìm kiếm không phân biệt chữ hoa chữ thường
                              .OrderBy(s => s.TenSanPham)  // Có thể thêm sắp xếp theo tên sản phẩm (hoặc theo tiêu chí khác)
                              .ToPagedList(pageNumber, pageSize);  // Phân trang kết quả tìm kiếm

            return View(results);  // Trả về kết quả phân trang
        }



    }
}