using DoAn.Areas.Admin.Models;
using DoAn.Common;
using DoAn.Models;
using System.Linq;
using System.Web.Mvc;

namespace DoAn.Areas.Admin.Controllers
{
    public class LoginsController : Controller
    {
        WebBanHangEntities _data = new WebBanHangEntities();

        // GET: /Admin/Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginUser(string UserName, string password)
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                ModelState.AddModelError(nameof(UserName), "Tên đăng nhập không được để trống."); // Username cannot be empty
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(nameof(password), "Mật khẩu không được để trống."); // Password cannot be empty
            }

            if (ModelState.IsValid)
            {
                var hashedPassword = EncryptorMD5.GetMD5(password);

                // Find user by either UserName or Email
                var user = _data.Users.FirstOrDefault(s =>
                    (s.UserName.Equals(UserName) || s.Email.Equals(UserName)) &&
                    s.Password.Equals(hashedPassword));

                if (user != null)
                {
                    if (user.Role != "Admin")
                    {
                        ViewBag.Message = "Bạn không có quyền truy cập vào hệ thống này."; // User does not have access
                        return View("Index");
                    }

                    Session["adminLogin"] = user;

                    // Redirect to admin home page after login
                    return RedirectToAction("Index", "Homes", new { area = "Admin" });
                }
                else
                {
                    ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng."; // Invalid credentials
                }
            }
            else
            {
                ViewBag.Message = "Vui lòng điền đầy đủ thông tin."; // Invalid form data
            }

            return View("Index");
        }

        public ActionResult Logout()
        {
            Session.Clear(); // Clear all session data
            return RedirectToAction("Index", "Logins", new { area = "Admin" });
        }
    }
}
