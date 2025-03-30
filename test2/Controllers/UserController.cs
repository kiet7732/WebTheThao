using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DoAn.Common;
using static DoAn.Areas.Admin.Controllers.UsersController;
using System.Data.Entity;
using System.Xml.Linq;
using System.Drawing.Printing;
using PagedList;
using static DoAn.Areas.Admin.Controllers.OrderController;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Net.Http;

namespace DoAn.Controllers
{
    public class UserController : Controller
    {

        WebBanHangEntities Data = new WebBanHangEntities();

        //string connectionString = ConfigurationManager.ConnectionStrings["Data"].ConnectionString;

        public ActionResult RegisterUser()
        {    
            return View();
        }

        // POST: Register
        [HttpPost]
        public ActionResult RegisterUser(Users _user)
        {
            // Check if the model state is valid
            ValidateUserFields(_user);
            _user.Role = "Customer";
            _user.Status = "Tài khoản mở";
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var check = Data.Users.FirstOrDefault(s => s.Email == _user.Email);
                var checkName = Data.Users.FirstOrDefault(s => s.UserName == _user.UserName);
                if (check == null && checkName == null)
                {
                    // Hash the password
                    _user.Password = EncryptorMD5.GetMD5(_user.Password);

                    try
                    {
                       
                        Data.Users.Add(_user);

                        Data.SaveChanges();

                        ViewBag.Message = "Đăng ký thành công";

                        SendConfirmationEmail(_user.Email, _user.TenNguoiDung);

                        return RedirectToAction("LoginUser", "User");
                    }
                    catch (Exception ex) // Generic exception handling
                    {
                        ViewBag.Message = "Đã xảy ra lỗi khi đăng ký: " + ex.Message;
                    }
                }
                else
                {
                    ViewBag.Message = "Email hoặc tên dăng nhập đã có!";
                }
            }
            return View();
        }


        private void SendConfirmationEmail(string email, string fullName)
        {
            var fromAddress = new MailAddress("2224801030058@student.tdmu.edu.vn", "Đăng ký thành công");
            var toAddress = new MailAddress(email, fullName);
            const string fromPassword = "Kiet7732"; // Use your app-specific password here
            const string subject = "Đăng Ký Thành Công";

            // Configure SMTP settings
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // Gmail SMTP server
                Port = 587, // Port for TLS
                EnableSsl = true, // Enable SSL
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword) // Your email credentials
            };

        
            string shopUrl = Url.Action("HomeShop", "Home", null, Request.Url.Scheme);

            // Email body
            string body = $"Chào {fullName},\n\n" +
                          "Cảm ơn bạn đã đăng ký tài khoản với chúng tôi.\n" +
                          $"Bấm vào đường link sau để tiếp tục mua sắm nhé: {shopUrl}";


            // Create the email message
            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
            {
                try
                {
                    smtp.Send(message); // Send the email
                    Console.WriteLine("Email sent successfully.");
                }
                catch (SmtpException smtpEx)
                {
                    // Handle any errors that occur during sending
                    Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                    if (smtpEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {smtpEx.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    Console.WriteLine($"General Error: {ex.Message}");
                }
            }
        }

        // check kiểu sdt
        private bool IsPhoneNumberValid(string phoneNumber)
        {
            // Check if the phone number contains only digits
            return phoneNumber.All(char.IsDigit);
        }

        private void ValidateUserFields(Users _user)
        {
            // Validate that required fields are not empty
            if (string.IsNullOrWhiteSpace(_user.UserName))
            {
                ModelState.AddModelError(nameof(_user.UserName), "Tên đăng nhập không được để trống."); // Username cannot be empty
            }
            if (string.IsNullOrWhiteSpace(_user.Password))
            {
                ModelState.AddModelError(nameof(_user.Password), "Mật khẩu không được để trống."); // Password cannot be empty
            }
            if (string.IsNullOrWhiteSpace(_user.ConfirmPassword))
            {
                ModelState.AddModelError(nameof(_user.ConfirmPassword), "Vui lòng xác nhận mật khẩu."); // Confirm password cannot be empty
            }
            else if (_user.Password != _user.ConfirmPassword) // Check if passwords match
            {
                ModelState.AddModelError(nameof(_user.ConfirmPassword), "Mật khẩu không trùng khớp."); // Passwords do not match
            }
            if (string.IsNullOrWhiteSpace(_user.Email))
            {
                ModelState.AddModelError(nameof(_user.Email), "Email không được để trống."); // Email cannot be empty
            }
            if (string.IsNullOrWhiteSpace(_user.SDT))
            {
                ModelState.AddModelError(nameof(_user.SDT), "Số điện thoại không được để trống."); // Phone number cannot be empty
            }
            else if (!IsPhoneNumberValid(_user.SDT))
            {
                ModelState.AddModelError(nameof(_user.SDT), "Số điện thoại không hợp lệ. Chỉ chứa số."); // Invalid phone number
            }
            else if (!_user.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError(nameof(_user.Email), "Email phải có đuôi @gmail.com."); // Email must end with @gmail.com
            }
            if (string.IsNullOrWhiteSpace(_user.TenNguoiDung))
            {
                ModelState.AddModelError(nameof(_user.TenNguoiDung), "Tên người dùng không được để trống."); // Phone number cannot be empty
            }
        }

        public ActionResult LoginUser()
        {
            var clientId = "23085717598-6tf7h8pv2qmqff9teag36pvj7ctese9j.apps.googleusercontent.com";
            var url = "http://shopkbadn.somee.com/User/LoginGoogle";
            string response = GenerateGoogleLeOAuthUrl(clientId, url);

            ViewBag.response = response;
            return View();
        }


        [HttpPost]
        public ActionResult LoginUser(string UserName, string password, string returnUrl) // Changed to UserName to match form input
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                ModelState.AddModelError(nameof(UserName), "Tên đăng nhập không được để trống."); // Username cannot be empty
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(nameof(password), "Mật khẩu không được để trống."); // Password cannot be empty
            }

            // Lấy URL của trang trước đó (Referrer)

            if (ModelState.IsValid)
            {
                var hashedPassword = EncryptorMD5.GetMD5(password); 

                // Find user by either UserName or Email
                var user = Data.Users.FirstOrDefault(s =>
                    (s.UserName.Equals(UserName) || s.Email.Equals(UserName)) &&
                    s.Password.Equals(hashedPassword));

                if (user != null)
                {
                    if (user.Role != "Customer")
                    {
                        ViewBag.Message = "Bạn không có quyền truy cập vào hệ thống này."; // User does not have access
                        return View();
                    }

                    //if (user.Status != "Tài khoản mở" )
                    //{
                    //    ViewBag.Message = "Tài khoản đã bị khóa!"; // User does not have access
                    //    return View();
                    //}

                    // Set session variables
                    Session["FullName"] = user.TenNguoiDung;
                    Session["idUser"] = user.IdUser;
                    Session["Username"] = user.UserName;
                    Session["Email"] = user.Email;
                    Session["SDT"] = user.SDT;

                    return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/");
                }
                else
                {
                    @ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng."; // Invalid credentials
                }
            }
            else
            {
                @ViewBag.Message = "Vui lòng điền đầy đủ thông tin."; // Invalid form data
            }

            // Return view with error messages
            return View();
        }

        public ActionResult Logout()
        {
            // Xóa session
            Session.Clear();

            // Lấy URL của trang trước đó (Referrer)
            string returnUrl = Request.UrlReferrer?.ToString();

            // Nếu có trang trước đó thì chuyển hướng về trang đó, nếu không thì chuyển đến trang mặc định
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("HomeShop", "Home"); // Trang mặc định nếu không có referrer
            }
        }

        [HttpGet]
        public ActionResult DanhSachHang(int page = 1, int pagesize = 3)
        {
            if (Session["idUser"] != null)
            {
                var userId = Session["idUser"].ToString();

                var orderUser = (from ctdh in Data.ChiTietDonHang
                                 join dh in Data.DonHang on ctdh.CodeOrder equals dh.CodeOrder
                                 where dh.IdUser == userId  
                                 orderby ctdh.NgayDatHang descending  
                                 select ctdh)
                                 .GroupBy(o => o.CodeOrder)  // Group by CodeOrder
                                 .Select(g => g.OrderByDescending(o => o.NgayDatHang).FirstOrDefault())  // Select latest entry for each CodeOrder
                                 .OrderByDescending(o => o.NgayDatHang) 
                                 .ToPagedList(page, pagesize);  // Apply pagination

                return View(orderUser);  
            }

            return RedirectToAction("LoginUser");  
        }

        [HttpGet]
        public ActionResult ChiTietHoaDon(string CodeOrder)
        {
            if (string.IsNullOrEmpty(CodeOrder))
            {
                return RedirectToAction("HomeShop", "Home");
            }

            var query = from dh in Data.DonHang
                        join ctdh in Data.ChiTietDonHang on dh.CodeOrder equals ctdh.CodeOrder
                        join sp in Data.SanPham on dh.IdSanPham equals sp.IdSanPham  // Join with SanPham to get product details
                        where dh.CodeOrder == CodeOrder
                        orderby ctdh.NgayDatHang descending
                        select new OrderDetail
                        {
                            PhuongThucThanhToan = ctdh.PhuongThucThanhToan,
                            DiaChi = ctdh.DiaChi,
                            CodeOrder = ctdh.CodeOrder,
                            NgayDatHang = (DateTime)ctdh.NgayDatHang,
                            TenNguoiNhan = ctdh.TenNguoiNhan,
                            SDT = ctdh.SDT,
                            TongTien = (decimal)ctdh.TongTien,
                            TinhTrangThanhToan = ctdh.TinhTrangThanhToan,
                            Status = ctdh.Status,
                            IdChiTietDonHang = ctdh.IdChiTietDonHang,

                            SanPhams = (from dh in Data.DonHang
                                        join sp in Data.SanPham on dh.IdSanPham equals sp.IdSanPham
                                        where dh.CodeOrder == CodeOrder
                                        select new ProductDetail
                                        {
                                            IdSanPham = sp.IdSanPham.ToString(),
                                            TenSP = sp.TenSanPham,
                                            GiaSanPham = sp.GiaSanPham,
                                            SoLuong = dh.SoLuong,
                                            LoaiSize = dh.LoaiSize
                                        }).ToList()
                        };

            return View(query);
        }

        [HttpGet]
        public ActionResult ThongTinND()
        {
            if (Session["idUser"] != null)
            {
                var userId = Session["idUser"].ToString();

                var user = Data.Users.Find(userId); 

                if (user != null)
                {
                    var model = new Users
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        SDT = user.SDT,
                        TenNguoiDung = user.TenNguoiDung,
                        Password = user.Password,
                    };

                    return View(model);
                }
            }
            return RedirectToAction("LoginUser");
        }

        [HttpPost]
        public ActionResult ThongTinND(Users model)
        {
            if (Session["idUser"] != null)
            {
                var userId = Session["idUser"].ToString();
                var user = Data.Users.Find(userId);

                if (user != null)
                {
                    // Cập nhật thông tin cơ bản
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.SDT = model.SDT;
                    user.TenNguoiDung = model.TenNguoiDung;

                    // Kiểm tra nếu có yêu cầu thay đổi mật khẩu
                    if (!string.IsNullOrEmpty(model.Password) && model.Password == model.ConfirmPassword)
                    {
                        var hashedPassword = EncryptorMD5.GetMD5(model.Password);
                        user.Password = hashedPassword;
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    Data.SaveChanges();
                    TempData["Message"] = "Cập Nhật Thông Tin Thành Công!";
                    return View();
                }
                else
                {
                    return RedirectToAction("LoginUser");
                }
            }
            else
            {
                return RedirectToAction("LoginUser");
            }

            return View(model);
        }















        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var user =  Data.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Message"] = "Không tìm thấy email này trong hệ thống.";
                return View();
            }

            var token = Guid.NewGuid().ToString(); // tạo token
            user.ResetPasswordToken = token;
            user.TokenExpiration = DateTime.Now.AddHours(1);

             Data.SaveChanges();

            string resetLink = Url.Action("ResetPassword", "User", new { token = token, email = email }, Request.Url.Scheme);
            string emailBody = $"Click vào liên kết sau để đặt lại mật khẩu của bạn: <a href='{resetLink}'>Đặt lại mật khẩu</a>";

            ForgotPasswordSendEmail(email, emailBody);

            TempData["Message"] = "Email đặt lại mật khẩu đã được gửi.";
            return View();
        }


        private void ForgotPasswordSendEmail(string email, string emailBody)
        {
            var fromAddress = new MailAddress("2224801030058@student.tdmu.edu.vn", "Đặt lại Mật khẩu");
            var toAddress = new MailAddress(email);
            const string fromPassword = "Kiet7732"; // Use your app-specific password here
            const string subject = "Đặt lại Mật khẩu";

            // Configure SMTP settings
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // Gmail SMTP server
                Port = 587, // Port for TLS
                EnableSsl = true, // Enable SSL
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword) // Your email credentials
            };

            // Create the email message
            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = emailBody, IsBodyHtml = true })
            {
                try
                {
                    smtp.Send(message); // Send the email
                    Console.WriteLine("Email sent successfully.");
                }
                catch (SmtpException smtpEx)
                {
                    // Handle any errors that occur during sending
                    Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                    if (smtpEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {smtpEx.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    Console.WriteLine($"General Error: {ex.Message}");
                }
            }
        }

        public ActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("HomeShop", "Home");
            }

            var model = new DatLaiMatKhauViewModel { Token = token, Email = email };
            return View(model);
        }


        // POST: Xử lý việc đặt lại mật khẩu
        [HttpPost]
        public ActionResult ResetPassword(DatLaiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy người dùng từ cơ sở dữ liệu theo email
            var user = Data.Users.FirstOrDefault(u => u.Email == model.Email);

            Console.WriteLine($"Token Expiration: {user.TokenExpiration}");
            Console.WriteLine($"Current Time: {DateTime.Now}");

            if (user != null && user.ResetPasswordToken == model.Token && user.TokenExpiration > DateTime.Now)
            {
                var hashedPassword = EncryptorMD5.GetMD5(model.NewPassword);

                // Đặt lại mật khẩu mới
                user.Password = hashedPassword;
                user.ResetPasswordToken = null; // Xóa token sau khi đã sử dụng
                user.TokenExpiration = null;

                Data.SaveChanges();

                TempData["Message"] = "Mật khẩu của bạn đã được đặt lại thành công.";
                return RedirectToAction("LoginUser");
            }
            else
            {
                TempData["Message"] = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return View();
            }
        }














        public ActionResult HuyDon(string idChiTietDonHang)
        {
            // Lấy đơn hàng theo id
            var chiTietDonHangs = Data.ChiTietDonHang.SingleOrDefault(p => p.IdChiTietDonHang == idChiTietDonHang);

            if (chiTietDonHangs == null)
            {
                // Nếu không tìm thấy đơn hàng
                TempData["Message"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("DanhSachHang");
            }
            // Lấy CodeOrder từ ChiTietDonHang để tìm đơn hàng liên quan
            string codeOrder = chiTietDonHangs.CodeOrder;
            chiTietDonHangs.Status = "Đơn Đã Hủy";

            // Cập nhật trạng thái của chi tiết đơn hàng
            var DonHang = Data.DonHang.Where(ct => ct.CodeOrder == codeOrder).ToList();

            foreach (var item in DonHang)
            {
                item.Status = "Đơn Đã Hủy";
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            Data.SaveChanges();

            // Thông báo thành công
            TempData["Message"] = "Đơn hàng đã được hủy thành công!";
            return RedirectToAction("DanhSachHang");
        }


        private string GenerateGoogleLeOAuthUrl(string clientId, string redirectUri)
        {
            // Base URL of Google OAuth
            string googleOAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";

            // Create query string
            var queryParams = new List<KeyValuePair<string, string>>
            {
            new KeyValuePair<string, string>("response_type", "code"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("scope", "openid email profile"),
            new KeyValuePair<string, string>("access_type", "online")
            };

            // Create URL by concatenating parameters
            string queryString = string.Join("&", queryParams.Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value)}"));
            return $"{googleOAuthUrl}?{queryString}";
        }

        private async Task<string> ExchangeCodeToTokenAsync(string code, string clientId, string redirectUri, string clientSecret)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            using (var client = new HttpClient())
            {

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var response = await client.PostAsync(tokenEndpoint, content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

                if (jsonResponse.error != null)
                {
                    throw new Exception($"Error exchanging code: {jsonResponse.error_description}");
                }
                return jsonResponse.access_token;
            }
        }

        private async Task<dynamic> GetGoogleUserInfoAsync(string accessToken)
        {
            string userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(userInfoEndpoint);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic userInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

                if (userInfo.error != null)
                {
                    throw new Exception($"Error fetching user info: {userInfo.error.message}");
                }

                return userInfo;
            }
        }

        public async Task<ActionResult> LoginGoogle(string code, string scope, string authuser, string prompt)
        {
            string redirectUri = "http://shopkbadn.somee.com/User/LoginGoogle";
            var clientId = "23085717598-6tf7h8pv2qmqff9teag36pvj7ctese9j.apps.googleusercontent.com";
            var clientSecret = "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";
            if (string.IsNullOrEmpty(code))
            {
                // Nếu không có mã ủy quyền, chuyển hướng về trang đăng nhập hoặc hiển thị lỗi
                return RedirectToAction("LoginUser", "User");
            }
            try
            {
                // 1. Trao đổi mã ủy quyền để lấy Access Token
                var accessToken = await ExchangeCodeToTokenAsync(code, clientId, redirectUri, clientSecret);

                // 2. Lấy thông tin người dùng
                var userInfo = await GetGoogleUserInfoAsync(accessToken);

                // 3. Lấy số điện thoại của người dùng
                //var phoneNumber = await GetPhoneNumberFromGoogleAsync(accessToken);

                string name = userInfo.name?.ToString();
                string email = userInfo.email?.ToString();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                {
                    ViewBag.Error = "Incomplete user information returned by Google.";
                    return RedirectToAction("LoginUser");
                }

                Users kh = Data.Users.SingleOrDefault(u => u.Email == email);
                if (kh == null)
                {
                    string pass = Guid.NewGuid().ToString("N").Substring(0, 10);
                    Users khNew = new Users
                    {
                        TenNguoiDung = name,
                        UserName = name,
                        Email = email,
                        Password = pass,
                        SDT = "000",
                        Status = "Tài khoản mở",
                        Role = "Customer"
                    };

                    Data.Users.Add(khNew);
                    Data.SaveChanges();
                    Session["FullName"] = khNew.TenNguoiDung;
                    Session["idUser"] = khNew.IdUser;
                }
                else
                {
                    Session["FullName"] = kh.TenNguoiDung;
                    Session["idUser"] = kh.IdUser;
                }
                TempData["Message"] = "Chúc mừng đăng nhập Email thành công";
                return RedirectToAction("HomeShop", "Home");
            }
            catch (DbEntityValidationException dbEx)
            {
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(validationResult => validationResult.ValidationErrors)
                    .Select(error => $"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Hiển thị lỗi chi tiết cho người dùng
                return Content($"Validation Errors: {fullErrorMessage}");
            }
        }


    }
}