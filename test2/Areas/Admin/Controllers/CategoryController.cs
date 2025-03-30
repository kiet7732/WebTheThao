using DoAn.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Admin/Category
        WebBanHangEntities _data = new WebBanHangEntities();

        public ActionResult Index(string searchString, int page = 1)
        {
            var loai = _data.LoaiSanPham.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                loai = loai.Where(s => s.TenLoaiSanPham.Contains(searchString));
            }

            // Số lượng sản phẩm hiển thị trên mỗi trang
            int pageSize = 4;

            return View(loai.OrderBy(s => s.IdLoaiSanPham).ToPagedList(page, pageSize));
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {

            var loai = _data.LoaiSanPham.Find(id);
            if (loai == null)
            {
                TempData["Message"] = "Không tồn tại!";
                return RedirectToAction("Index");
            }

            return View(loai);
        }

        [HttpPost]
        public ActionResult Edit(LoaiSanPham model)
        {
            // Tìm sản phẩm theo ID
            var loai = _data.LoaiSanPham.Find(model.IdLoaiSanPham);
            if (loai == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return View(model);
            }

            loai.TenLoaiSanPham = model.TenLoaiSanPham;

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
        public ActionResult Create(LoaiSanPham model)
        {
            var loai = new LoaiSanPham();
            if (model == null)
            {
                TempData["Message"] = "Lỗi";
                return View(model);
            }

            loai.IdLoaiSanPham = Guid.NewGuid().ToString();
            loai.TenLoaiSanPham = model.TenLoaiSanPham;

            _data.SaveChanges();
            TempData["Message"] = "Thêm thành công";
            return View();
        }

    }
}