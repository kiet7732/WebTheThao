using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class ShopController : Controller
    {

        WebBanHangEntities Data = new WebBanHangEntities();

        public ActionResult index()
        {
            return View();
        }

        public ActionResult DetailProduct(string id)
        {
            var data = Data.SanPham
                .SingleOrDefault(p => p.IdSanPham == id);
            if (data == null)
            {
                TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }

            var result = new SanPham
            {
                IdSanPham = data.IdSanPham,
                TenSanPham = data.TenSanPham,
                LinkHinhAnh = data.LinkHinhAnh,
                MoTaSanPham = data.MoTaSanPham ?? string.Empty,
                SoLuongTon = data.SoLuongTon,
                GiaSanPham = data.GiaSanPham,
                NgayRaMat = data.NgayRaMat,
                IdSizeSp = data.IdSizeSp,
            };
            return View(result);
        }

    }
}