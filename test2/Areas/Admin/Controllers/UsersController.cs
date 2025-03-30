using DoAn.Common;
using DoAn.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Areas.Admin.Controllers
{
    public class UsersController : BaseController
    {
        // GET: Admin/User
        WebBanHangEntities _data = new WebBanHangEntities();

        public class UserDraw
        {
            WebBanHangEntities _data = new WebBanHangEntities();
            public UserDraw()
            {
                _data = new WebBanHangEntities();
            }
            public int coutUser()
            {
                return _data.Users.Count();
            }
        }
        public ActionResult Index(string searchString, int page = 1)
        {
            var Users = _data.Users.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                Users = Users.Where(s => s.UserName.Contains(searchString));
            }

            // Số lượng sản phẩm hiển thị trên mỗi trang
            int pageSize = 5;

            return View(Users.OrderBy(s => s.UserName).ToPagedList(page, pageSize));
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var Users = _data.Users.Find(id);
            if (Users == null)
            {
                TempData["Message"] = "Xoá Không Thành Công";
                return Redirect("Index");
            }

            _data.Users.Remove(Users);
            _data.SaveChanges();

            TempData["Message"] = "Xoá Thành Công";
            return RedirectToAction("Index", "Users");
        }

        public ActionResult Edit(string id)
        {
            var user = _data.Users.Find(id);  // Replace _data with your actual DbContext or repository

            if (user == null)
            {
                TempData["Message"] = "Người Dùng không tồn tại!";
                return RedirectToAction("Index");
            }

            return View(user);  // Pass the user data to the view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Users user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _data.Users.Find(user.IdUser);  

                if (existingUser != null)
                {
                    existingUser.UserName = user.UserName;
                    existingUser.TenNguoiDung = user.TenNguoiDung;
                    existingUser.Password = user.Password;
                    existingUser.Email = user.Email;
                    existingUser.SDT = user.SDT;

                    _data.SaveChanges();

                    TempData["Message"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Message"] = "Người Dùng không tồn tại!";
                    return RedirectToAction("Index");
                }
            }

            return View(user);
        }

       public ActionResult Lock(string id)
        {
            var Users = _data.Users.Find(id);

            if (Users != null)
            {

                if (Users.Status == "Tài khoản mở")
                {
                    Users.Status = "Tài khoản bị khóa";
                    TempData["Message"] = "Tài khoản bị khóa xác nhận";
                }
                else
                {
                    Users.Status = "Tài khoản mở";
                    TempData["Message"] = "Tài khoản đã mở";

                }
                _data.SaveChanges();
                return RedirectToAction("Index");

            }

            TempData["Message"] = "Đơn hàng không tồn tại.";
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        public ActionResult Create(Users model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    // Thêm thông báo lỗi nếu mật khẩu không trùng khớp
                    ViewBag.Message = "Mật khẩu và xác nhận mật khẩu không khớp.";
                    return View(model);
                }

                // Kiểm tra nếu tên người dùng đã tồn tại
                var existingUser = _data.Users.FirstOrDefault(u => u.UserName == model.UserName);
                if (existingUser != null)
                {
                    ViewBag.Message = "Tên người dùng đã tồn tại!";
                    return View(model);
                }

                var hashedPassword = EncryptorMD5.GetMD5(model.Password);

                // Đặt giá trị mặc định cho các trường nếu cần
                model.Role = "Admin";  // Gán giá trị "Admin" cho Role
                model.SDT = "a";       // Gán giá trị mặc định
                model.TenNguoiDung = "a"; // Gán giá trị mặc định
                model.Email = "a";     // Gán giá trị mặc định

                model.UserName = model.UserName;
                model.Password = hashedPassword;

                // Lưu vào database
                _data.Users.Add(model);
                _data.SaveChanges();

                // Chuyển hướng sau khi tạo thành công
                TempData["Message"] = "Tạo Thành Công!";
                return RedirectToAction("Index", "Homes"); // Hoặc đến một trang khác như Dashboard
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form với thông báo lỗi
            return View(model);
        }

    }
}