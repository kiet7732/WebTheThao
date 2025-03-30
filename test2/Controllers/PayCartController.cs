using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Newtonsoft.Json;

namespace DoAn.Controllers
{
    public class PayCartController : Controller
    {
        WebBanHangEntities Data = new WebBanHangEntities();

        [HttpGet]
        public ActionResult GioHang()
        { 
            var userId = Session["idUser"];

            if (userId == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("LoginUser", "User"); 
            }

            var cartItems = (from g in Data.GioHang
                             join p in Data.SanPham on g.IdSanPham equals p.IdSanPham
                             where g.IdUser == userId
                             select new CartItem
                             {
                                 IdGioHang = g.IdGioHang,
                                 SoLuong = g.SoLuong,
                                 LoaiSize = g.TLoaiSize,

                                 GiaSanPham = p.GiaSanPham,
                                 TenSanPham = p.TenSanPham,
                                 LinkHinhAnh = p.LinkHinhAnh,
                                 IdSizeSp = p.IdSizeSp
                             }).ToList();

            return View(cartItems);
        }

        [HttpGet]
        public ActionResult RemoveFromCart(string idGioHang)
        {
            var cartItem = Data.GioHang.Find(idGioHang);

            if (cartItem != null)
            {
                TempData["Message"] = "Xóa thành công";
                Data.GioHang.Remove(cartItem);
                Data.SaveChanges(); // Save changes
            }

            return RedirectToAction("GioHang"); // Redirect back to the cart view
        }

        [HttpGet]
        public ActionResult IncreaseQuantity(string idGioHang)
        {
            var item = Data.GioHang.Find(idGioHang);

            if (item != null)
            {
                item.SoLuong += 1;
                Data.SaveChanges();
            }
            return RedirectToAction("GioHang");
        }

        [HttpGet]
        public ActionResult DecreaseQuantity(string idGioHang)
        {
            var item = Data.GioHang.Find(idGioHang);

            if (item != null)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong -= 1;
                    Data.SaveChanges();
                }
                else if (item.SoLuong == 1)
                {
                    TempData["Message"] = "Xóa thành công";
                    // If quantity is 1 and decrease is called, remove the item
                    Data.GioHang.Remove(item);
                    Data.SaveChanges();
                }
            }
            return RedirectToAction("GioHang");
        }

        public ActionResult CheckOut()
        {
            var userId = Session["idUser"];

            if (userId == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("LoginUser", "User");
            }

            var cartItems = (from g in Data.GioHang
                             join p in Data.SanPham on g.IdSanPham equals p.IdSanPham
                             where g.IdUser == userId
                             select new CartItem
                             {
                                 IdGioHang = g.IdGioHang,
                                 SoLuong = g.SoLuong,
                                 LoaiSize = g.TLoaiSize,

                                 GiaSanPham = p.GiaSanPham,
                                 TenSanPham = p.TenSanPham,
                                 IdSizeSp = p.IdSizeSp
                             }).ToList();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống!";
                return Redirect("GioHang"); // Adjust "Cart" to the correct controller if different
            }

            var itemNeedingSize = cartItems.FirstOrDefault(item => item.IdSizeSp != "SIZENONE" && item.LoaiSize == null);

            if (itemNeedingSize != null)
            {
                TempData["Message"] = $"Sản phẩm '{itemNeedingSize.TenSanPham}' yêu cầu chọn size.";
                return Redirect("GioHang"); // Adjust as necessary
            }

            ViewBag.Email = Session["Email"];
            ViewBag.SDT = Session["SDT"];

            return View(cartItems);
        }

        private bool CheckPhoneNumber(string phone)
        {
            // Ensure phone number is not empty, contains only digits, and has a length between 1 and 11 characters
            return !string.IsNullOrEmpty(phone) && phone.Length <= 11 && phone.All(char.IsDigit);
        }


        [HttpPost]
        public ActionResult PlaceOrder(C_DonHang model, string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Message"] = "Không nhận được phương thức thanh toán.";
                return RedirectToAction("CheckOut");
            }

            var userId = Session["idUser"];
            if (userId == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập để đặt hàng.";
                return RedirectToAction("LoginUser", "User");
            }

            // Loại bỏ tất cả các khoảng trắng trong số điện thoại
            model.Phone = model.Phone.Replace(" ", "");
            if (!CheckPhoneNumber(model.Phone))
            {
                TempData["Message"] = "Số điện thoại không hợp lệ. Vui lòng nhập lại (9 chữ số).";
                return Redirect("CheckOut");
            }

            string TT = "Chưa thanh toán";
            string PT = "Tiền mặt";

            if (paymentMethod == "chuyenkhoang")
            {
                HttpContext.Session.Add("QROrderDetails", model);
                return Redirect("QRPayment");
            }

            if (paymentMethod == "chuyenkhoangForm")
            {
                TT = "Đã thanh toán";
                PT = "Chuyển khoản";
            }

            // Calculate total price from cart
            var cartItems = (from g in Data.GioHang
                             join p in Data.SanPham on g.IdSanPham equals p.IdSanPham
                             where g.IdUser == userId
                             select new CartItem
                             {
                                 IdGioHang = g.IdGioHang,
                                 SoLuong = g.SoLuong,
                                 LoaiSize = g.TLoaiSize,
                                 IdSanPham = p.IdSanPham,
                                 GiaSanPham = p.GiaSanPham,
                                 TenSanPham = p.TenSanPham,
                                 IdSizeSp = p.IdSizeSp
                             }).ToList();

            decimal totalAmount = cartItems.Sum(item => item.GiaSanPham * item.SoLuong);
            string codeOrder = CodeOrder();

            // Save order details
            var orderDetail = new ChiTietDonHang
            {
                NgayDatHang = DateTime.Now,
                TenNguoiNhan = $"{model.FirstName} {model.LastName}",
                SDT = model.Phone,
                TongTien = totalAmount,
                DiaChi = $"{model.Country} {model.Address} {model.Apartment}",
                Email = model.EmailAddress,
                TinNhan = model.OrderNotes,
                CodeOrder = codeOrder,
                Status = "Chờ xác nhận",
                TinhTrangThanhToan = TT,
                PhuongThucThanhToan = PT
            };

            Data.ChiTietDonHang.Add(orderDetail);
            try
            {
                Data.SaveChanges();

            }
            catch(DbEntityValidationException e)
            {
                Console.WriteLine(e);
            }

            // Save cart items to the order
            SaveCartItemsToOrder(cartItems, userId.ToString(), codeOrder, model.OrderNotes, orderDetail);

            // Clear cart
            ClearCartItems(userId.ToString());

            return RedirectToAction("DoneCheckOut");
        }

        private void SaveCartItemsToOrder(List<CartItem> cartItems, string userId, string codeOrder, string orderNotes, ChiTietDonHang orderDetail)
        {
            foreach (var item in cartItems)
            {
                var donHang = new DonHang
                {
                    IdSanPham = item.IdSanPham,
                    GiaSanPham = item.GiaSanPham,
                    SoLuong = item.SoLuong,
                    LoaiSize = item.LoaiSize,
                    TinNhan = orderNotes,
                    IdUser = userId,
                    CodeOrder = codeOrder,
                    Status = "Chờ Xác Nhận"
                };

                Data.DonHang.Add(donHang);
            }

            SendConfirmationEmail(orderDetail.Email, orderDetail.TenNguoiNhan, codeOrder, orderNotes, cartItems);
            Data.SaveChanges();
        }

        private void SendConfirmationEmail(string email, string fullName, string codeOrder, string orderNotes, List<CartItem> cartItems)
        {
            var fromAddress = new MailAddress("2224801030058@student.tdmu.edu.vn", "Xác Nhận Đơn Hàng");
            var toAddress = new MailAddress(email, fullName);
            const string fromPassword = "Kiet7732";
            const string subject = "Xác Nhận Đơn Hàng";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            var totalAmount = cartItems.Sum(p => p.SoLuong * p.GiaSanPham);
            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<h2 style='color: #2C3E50;'>Xác Nhận Đơn Hàng</h2>");
            bodyBuilder.AppendLine($"<p>Xin chào <strong>{fullName}</strong>,</p>");
            bodyBuilder.AppendLine("<p>Đơn hàng của bạn đã được ghi nhận. Dưới đây là chi tiết đơn hàng:</p>");

            bodyBuilder.AppendLine("<table border='1' cellpadding='10' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-top: 20px;'>");
            bodyBuilder.AppendLine("<tr style='background-color: #f4f4f4; font-weight: bold; text-align: center;'>");
            bodyBuilder.AppendLine("<th>STT</th><th>Tên sản phẩm</th><th>Loại Size</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th>");
            bodyBuilder.AppendLine("</tr>");

            int STT = 1;
            foreach (var product in cartItems)
            {
                var productTotal = product.SoLuong * product.GiaSanPham;
                bodyBuilder.AppendLine("<tr>");
                bodyBuilder.AppendLine($"<td style='text-align: center;'>{STT++}</td>");
                bodyBuilder.AppendLine($"<td>{product.TenSanPham}</td>");
                bodyBuilder.AppendLine($"<td style='text-align: center;'>{product.LoaiSize}</td>");
                bodyBuilder.AppendLine($"<td style='text-align: center;'>{product.SoLuong}</td>");
                bodyBuilder.AppendLine($"<td style='text-align: right;'>{product.GiaSanPham:N0} VNĐ</td>");
                bodyBuilder.AppendLine($"<td style='text-align: right;'>{productTotal:N0} VNĐ</td>");
                bodyBuilder.AppendLine("</tr>");
            }

            bodyBuilder.AppendLine("</table>");
            bodyBuilder.AppendLine($"<p><strong>Tổng tiền: {totalAmount:N0} VNĐ</strong></p>");
            bodyBuilder.AppendLine($"<p><strong>Tin nhắn:</strong> {orderNotes}</p>");
            bodyBuilder.AppendLine("<p><strong>Trạng thái:</strong> Chờ Xác Nhận</p>");
            bodyBuilder.AppendLine("<p>Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất và giao hàng đúng hẹn.</p>");
            bodyBuilder.AppendLine("<p>Trân trọng,</p>");
            bodyBuilder.AppendLine("<p>Đội ngũ Xác Nhận Đơn Hàng</p>");

            string body = bodyBuilder.ToString();

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                try
                {
                    smtp.Send(message);
                    Console.WriteLine("Email sent successfully.");
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                    if (smtpEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {smtpEx.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error: {ex.Message}");
                }
            }
        }




        private void ClearCartItems(string userId)
        {
            var cartToDelete = Data.GioHang.Where(g => g.IdUser == userId).ToList();
            Data.GioHang.RemoveRange(cartToDelete);
            Data.SaveChanges(); // Save changes to remove items from the cart
        }

        private string CodeOrder()
        {
            // Lấy thời gian hiện tại
            var currentDateTime = DateTime.Now;

            // Tạo mã đơn hàng dựa trên năm, tháng, ngày, giờ, phút, giây và một số ngẫu nhiên
            string orderCode = $"ORDER-{currentDateTime:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";

            return orderCode;
        }
        //end xu ly dat hang


        public ActionResult DoneCheckOut()
        {
            return View();
        }



        [HttpGet]
        public ActionResult ReSize(string idGioHang, string LoaiSize)  //chọn size
        {
            var item = Data.GioHang.Find(idGioHang);

            if (item != null)
            {
                // Convert the string 'NULL' to a proper representation
                item.TLoaiSize = LoaiSize == "NULL" ? null : LoaiSize; // Use null for database NULL
                Data.SaveChanges();
            }

            return RedirectToAction("GioHang");
        }

        [HttpGet]
        public ActionResult QRPayment()
        {
            //lay data nhap
            var model = HttpContext.Session["QROrderDetails"] as C_DonHang;

            if (model == null)
            {
                TempData["Message"] = "Thông tin đơn hàng không hợp lệ.";
                return RedirectToAction("CheckOut");
            }

            var userId = Session["idUser"];
            if (userId == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập.";
                return RedirectToAction("LoginUser", "User");
            }

            ViewBag.FirstName = model.FirstName;
            ViewBag.LastName = model.LastName;
            ViewBag.Address = model.Address;
            ViewBag.Apartment = model.Apartment;
            ViewBag.SDT = model.Phone;
            ViewBag.Email = model.EmailAddress;
            ViewBag.OrderNotes = model.OrderNotes;

            var cartItems = (from g in Data.GioHang
                             join p in Data.SanPham on g.IdSanPham equals p.IdSanPham
                             where g.IdUser == userId
                             select new CartItem
                             {
                                 IdGioHang = g.IdGioHang,
                                 SoLuong = g.SoLuong,
                                 LoaiSize = g.TLoaiSize,
                                 IdSanPham = p.IdSanPham,
                                 GiaSanPham = p.GiaSanPham,
                                 TenSanPham = p.TenSanPham,
                                 IdSizeSp = p.IdSizeSp
                             }).ToList();

            decimal totalAmount = cartItems.Sum(item => item.GiaSanPham * item.SoLuong);

            ViewBag.totalAmount = totalAmount;

            return View(model);
        }



    }
}