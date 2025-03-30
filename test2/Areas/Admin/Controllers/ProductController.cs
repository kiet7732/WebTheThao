using DoAn.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace DoAn.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        // GET: Admin/Product
        WebBanHangEntities _data = new WebBanHangEntities();
        public class ProductDraw
        {
            WebBanHangEntities _data = new WebBanHangEntities();
            public ProductDraw()
            {
                _data = new WebBanHangEntities();
            }
            public int coutProduct()
            {
                return _data.SanPham.Count();
            }
        }


        public ActionResult Index(string searchString, int page = 1)
        {
            var sanpham = _data.SanPham.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                sanpham = sanpham.Where(s => s.TenSanPham.Contains(searchString));
            }

            // Số lượng sản phẩm hiển thị trên mỗi trang
            int pageSize = 4;

            return View(sanpham.OrderBy(s => s.IdSanPham).ToPagedList(page, pageSize));
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var product = _data.SanPham.Find(id);
            if (product == null)
            {
                TempData["Message"] = "Xoá Không Thành Công";
                return Redirect("Index");
            }

            _data.SanPham.Remove(product);
            _data.SaveChanges();

            TempData["Message"] = "Xoá Thành Công";
            return RedirectToAction("Index", "Product");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {

            ViewBag.Loai = new SelectList(_data.LoaiSanPham.ToList().OrderBy(n => n.TenLoaiSanPham), "IdLoaiSanPham", "TenLoaiSanPham");
            ViewBag.Hang = new SelectList(_data.HangSanPham.ToList().OrderBy(n => n.TenHangSanPham), "IdHangSanPham", "TenHangSanPham");
            ViewBag.Size = new SelectList(_data.KichCo.ToList().OrderBy(n => n.LoaiSize), "IdSizeSp", "LoaiSize");

            var product = _data.SanPham.Find(id);
            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // POST: Product/Edit/{id}
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(SanPham model, FormCollection f, HttpPostedFileBase fFileUpload)
        {
            // Lấy các dữ liệu cần thiết cho Dropdown từ ViewBag
            ViewBag.Loai = new SelectList(_data.LoaiSanPham.ToList().OrderBy(n => n.TenLoaiSanPham), "IdLoaiSanPham", "TenLoaiSanPham", model.LoaiSanPham);
            ViewBag.Hang = new SelectList(_data.HangSanPham.ToList().OrderBy(n => n.TenHangSanPham), "IdHangSanPham", "TenHangSanPham", model.HangSanPham);
            ViewBag.Size = new SelectList(_data.KichCo.ToList().OrderBy(n => n.LoaiSize), "IdSizeSp", "LoaiSize", model.IdSizeSp);

            // Tìm sản phẩm theo ID
            var product = _data.SanPham.Find(model.IdSanPham);
            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return View(model);
            }

            ViewBag.ThongBao1 = product.LinkHinhAnh;

            // Kiểm tra xem có ảnh mới không
            if (fFileUpload != null && fFileUpload.ContentLength > 0)
            {
                var sFileName = Path.GetFileName(fFileUpload.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Images"), sFileName);
                if (System.IO.File.Exists(path))
                {
                    fFileUpload.SaveAs(path);  // Lưu ảnh vào thư mục Content/Images
                    product.LinkHinhAnh = Path.Combine("~/Content/Images//", sFileName); ;  // Cập nhật link hình ảnh cho sản phẩm
                }
            }
            string loaiSize = f["LoaiSize"];

            // Cập nhật các thông tin sản phẩm
            product.TenSanPham = f["TenSanPham"];
            product.HangSanPham = f["HangSanPham"];
            product.LoaiSanPham = f["LoaiSanPham"];
            product.KichCo = _data.KichCo.FirstOrDefault(k => k.LoaiSize == loaiSize);
            product.MoTaSanPham = f["MoTaSanPham"];
            product.GiaSanPham = decimal.Parse(f["GiaSanPham"]);
            product.SoLuongTon = int.Parse(f["SoLuongTon"]);

            // Lưu thay đổi vào cơ sở dữ liệu
            _data.SaveChanges();

            TempData["Message"] = "Cập nhật thông tin sản phẩm thành công!";
            return RedirectToAction("Index");
        }


        public ActionResult Create()
        {
            ViewBag.Loai = new SelectList(_data.LoaiSanPham.ToList().OrderBy(n => n.TenLoaiSanPham), "IdLoaiSanPham", "TenLoaiSanPham");
            ViewBag.Hang = new SelectList(_data.HangSanPham.ToList().OrderBy(n => n.TenHangSanPham), "IdHangSanPham", "TenHangSanPham");
            ViewBag.Size = new SelectList(_data.KichCo.ToList().OrderBy(n => n.LoaiSize), "IdSizeSp", "LoaiSize");

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(SanPham model, FormCollection f, HttpPostedFileBase fFileUpload)
        {
            // Lấy các dữ liệu cần thiết cho Dropdown từ ViewBag
            ViewBag.Loai = new SelectList(_data.LoaiSanPham.ToList().OrderBy(n => n.TenLoaiSanPham), "IdLoaiSanPham", "TenLoaiSanPham", model.LoaiSanPham);
            ViewBag.Hang = new SelectList(_data.HangSanPham.ToList().OrderBy(n => n.TenHangSanPham), "IdHangSanPham", "TenHangSanPham", model.HangSanPham);
            ViewBag.Size = new SelectList(_data.KichCo.ToList().OrderBy(n => n.LoaiSize), "IdSizeSp", "LoaiSize", model.IdSizeSp);

            if (model == null)
            {
                TempData["Message"] = "Lỗi";
                return View(model);
            }

            ViewBag.ThongBao1 = model.LinkHinhAnh;

            // Kiểm tra xem có ảnh mới không
            if (fFileUpload != null && fFileUpload.ContentLength > 0)
            {
                var sFileName = Path.GetFileName(fFileUpload.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Images"), sFileName);
                if (System.IO.File.Exists(path))
                {
                    fFileUpload.SaveAs(path);  // Lưu ảnh vào thư mục Content/Images
                    model.LinkHinhAnh = Path.Combine("~/Content/Images//", sFileName); ;  // Cập nhật link hình ảnh cho sản phẩm
                }
            }

            string loaiSize = f["LoaiSize"];
            // Cập nhật các thông tin sản phẩm
            model.TenSanPham = f["TenSanPham"];
            model.HangSanPham = f["HangSanPham"];
            model.LoaiSanPham = f["LoaiSanPham"];
            model.KichCo = _data.KichCo.FirstOrDefault(k => k.LoaiSize == loaiSize);
            model.MoTaSanPham = f["MoTaSanPham"];
            model.GiaSanPham = decimal.Parse(f["GiaSanPham"]);
            model.SoLuongTon = int.Parse(f["SoLuongTon"]);
            model.NgayRaMat = DateTime.Now;
            model.IdSanPham = "â";
            // Lưu thay đổi vào cơ sở dữ liệu
            try
            {
                _data.SanPham.Add(model);
                _data.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Console.Write(e);
                //TempData["Message"] = "Lỗi khi lưu sản phẩm: " + ex.Message;
                return View(model);
            }


            TempData["Message"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult NgungBan(string id)
        {
            var product = _data.SanPham.Find(id);
            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index");
            }
            if(product.LinkSanPham != "Ngừng Bán")
            {
                product.LinkSanPham = "Ngừng Bán";
            }
            else
            {
                product.LinkSanPham = "Đang Bán";

            }

            _data.SaveChanges();
            TempData["Message"] = "Thay đổi thành công!";
            return RedirectToAction("Index");
        }

    }
}