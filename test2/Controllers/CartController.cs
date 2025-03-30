using DoAn.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace DoAn.Controllers
{
    public class CartController : Controller
    {
        private readonly WebBanHangEntities _context = new WebBanHangEntities();

        [HttpGet]
        public ActionResult AddToCart(string idSanPham, string loaiSize)
        {
            try
            {
                var userId = Session["idUser"];
                var product = _context.SanPham.Find(idSanPham);

                if (userId == null )
                {
                    TempData["Message"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!";
                    return Redirect(Request.UrlReferrer.ToString());
                }

                if (product == null)
                {
                    TempData["Message"] = "Sản phẩm không tồn tại";
                    return Redirect(Request.UrlReferrer.ToString());
                }

                if (product.LinkSanPham == "Ngừng Bán" || product.SoLuongTon < 0)
                {
                    TempData["Message"] = "Sản phẩm đã ngừng bán hoặc hết hàng!";
                    return Redirect(Request.UrlReferrer.ToString());
                }

                var userIdString = userId.ToString();
                var existingCartItem = _context.GioHang
                    .FirstOrDefault(c => c.IdUser == userIdString && c.IdSanPham == idSanPham);

                if (existingCartItem != null)
                {
                    // Update quantity and optional size
                    existingCartItem.SoLuong += 1;
                    existingCartItem.TLoaiSize = string.IsNullOrEmpty(loaiSize) ? null : loaiSize;
                }
                else
                {
                    // Create a new cart item
                    var newCartItem = new GioHang
                    {
                        IdUser = userIdString,
                        IdSanPham = idSanPham,
                        SoLuong = 1,
                        TLoaiSize = string.IsNullOrEmpty(loaiSize) ? null : loaiSize
                    };
                    _context.GioHang.Add(newCartItem);
                }

                _context.SaveChanges();

                TempData["Message"] = "Thêm sản phẩm vào giỏ hàng thành công!";
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return RedirectToAction("HomeShop", "Home");
            }
        }
    }
}
