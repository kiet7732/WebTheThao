using DoAn.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Areas.Admin.Controllers
{
    public class CategoryHangController : Controller
    {
        // GET: Admin/CategoryHang

        WebBanHangEntities _data = new WebBanHangEntities();

        public ActionResult Index(string searchString, int page = 1)
        {
            var loai = _data.HangSanPham.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                loai = loai.Where(s => s.TenHangSanPham.Contains(searchString));
            }

            // Số lượng sản phẩm hiển thị trên mỗi trang
            int pageSize = 4;

            return View(loai.OrderBy(s => s.IdHangSanPham).ToPagedList(page, pageSize));
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {

            var loai = _data.HangSanPham.Find(id);
            if (loai == null)
            {
                TempData["Message"] = "Không tồn tại!";
                return RedirectToAction("Index");
            }

            return View(loai);
        }

        [HttpPost]
        public ActionResult Edit(HangSanPham model)
        {
            // Tìm sản phẩm theo ID
            var loai = _data.HangSanPham.Find(model.IdHangSanPham);
            if (loai == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return View(model);
            }

            loai.TenHangSanPham = model.TenHangSanPham;

            // Lưu thay đổi vào cơ sở dữ liệu
            _data.SaveChanges();

            TempData["Message"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HangSanPham model)
        {
            var loai = new HangSanPham();
            if (model == null)
            {
                TempData["Message"] = "Lỗi";
                return View(model);
            }

            loai.IdHangSanPham = Guid.NewGuid().ToString();
            loai.TenHangSanPham = model.TenHangSanPham;

            _data.SaveChanges();
            TempData["Message"] = "Thêm thành công";
            return View();
        }

        //[HttpGet]
        //public ActionResult Delete(string id)
        //{
        //    var loai = _data.HangSanPham.Find(id);
        //    if (loai == null)
        //    {
        //        viết câu lệnh xóa ở đâu
        //        return RedirectToAction("Index");
        //    }

        //    // Kiểm tra nếu đang liên kết
        //    var isLinked = _data.OtherTable.Any(x => x.HangSanPhamId == id);
        //    if (isLinked)
        //    {
        //        // Gửi thông báo cho người dùng
        //        TempData["ErrorMessage"] = "Không thể xóa vì có liên kết với dữ liệu khác.";
        //        return RedirectToAction("Index");
        //    }

        //    return View(loai);
        //}

    }
}