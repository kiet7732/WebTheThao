using DoAn.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Drawing;

namespace DoAn.Areas.Admin.Controllers
{
    public class OrderController : BaseController
    {
        // GET: Admin/Order
        WebBanHangEntities _data = new WebBanHangEntities();

        public class OrderDraw
        {
            WebBanHangEntities _data = new WebBanHangEntities();
            public OrderDraw()
            {
                _data = new WebBanHangEntities();
            }
            public int coutOrder()
            {
                return _data.ChiTietDonHang.Count();
            }
        }

        public ActionResult Index(string searchString, int page = 1)
        {
            var ChiTietDonHang = _data.ChiTietDonHang.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                ChiTietDonHang = ChiTietDonHang.Where(s => s.TenNguoiNhan.Contains(searchString));
            }

            // Số lượng sản phẩm hiển thị trên mỗi trang
            int pageSize = 5;

            return View(ChiTietDonHang.OrderBy(s => s.IdChiTietDonHang).ToPagedList(page, pageSize));
        }

        public ActionResult Delete(string id)
        {
            var ChiTietDonHang = _data.ChiTietDonHang.Find(id);
            if (ChiTietDonHang == null)
            {
                TempData["Message"] = "Xoá Không Thành Công";
                return Redirect("Index");
            }

            _data.ChiTietDonHang.Remove(ChiTietDonHang);
            _data.SaveChanges();

            TempData["Message"] = "Xoá Thành Công";
            return RedirectToAction("Index", "Order");
        }
        
        public ActionResult Detaill(string CodeOrder)
        {
            if (string.IsNullOrEmpty(CodeOrder))
            {
                TempData["Message"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            var query = from dh in _data.DonHang
                        join ctdh in _data.ChiTietDonHang on dh.CodeOrder equals ctdh.CodeOrder
                        join sp in _data.SanPham on dh.IdSanPham equals sp.IdSanPham  // Join with SanPham to get product details
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
                            Email = ctdh.Email,
                            TinNhan = ctdh.TinNhan,

                            SanPhams = (from dh in _data.DonHang
                                        join sp in _data.SanPham on dh.IdSanPham equals sp.IdSanPham
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

        public ActionResult ExportToExcel(string CodeOrder)
        {
            // Set license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Get order details
            var orderDetails = from dh in _data.DonHang
                                join ctdh in _data.ChiTietDonHang on dh.CodeOrder equals ctdh.CodeOrder
                                join sp in _data.SanPham on dh.IdSanPham equals sp.IdSanPham  // Join with SanPham to get product details
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
                                    Email = ctdh.Email,
                                    TinNhan = ctdh.TinNhan,

                                    SanPhams = (from dh in _data.DonHang
                                                join sp in _data.SanPham on dh.IdSanPham equals sp.IdSanPham
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

            if (!orderDetails.Any())
            {
                return new HttpStatusCodeResult(404, "Không tìm thấy đơn hàng.");
            }

            string templatePath = Server.MapPath("~/Areas/Admin/Excel/TemplateHoaDon.xlsx");
            string outputPath = Server.MapPath($"~/Areas/Admin/Excel/User/DonHang-{CodeOrder}.xlsx");

            // Create output directory if not exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Styling constants
                var headerStyle = package.Workbook.Styles.CreateNamedStyle("HeaderStyle");
                headerStyle.Style.Font.Bold = true;
                headerStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerStyle.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
                headerStyle.Style.Font.Color.SetColor(Color.White);
                headerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Set the title
                worksheet.Cells["A1:G1"].Merge = true;
                worksheet.Cells["A1"].Value = "CHI TIẾT ĐƠN HÀNG";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Order Information Section
                var order = orderDetails.First();
                int currentRow = 3;

                // Left column information
                worksheet.Cells[currentRow, 1].Value = "THÔNG TIN ĐƠN HÀNG";
                worksheet.Cells[currentRow, 1, currentRow, 3].Merge = true;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                currentRow += 2;
                AddOrderInfoRow(worksheet, ref currentRow, "Mã đơn hàng:", order.IdChiTietDonHang);
                AddOrderInfoRow(worksheet, ref currentRow, "Ngày đặt hàng:", order.NgayDatHang.ToString("dd/MM/yyyy HH:mm"));
                AddOrderInfoRow(worksheet, ref currentRow, "Tên người nhận:", order.TenNguoiNhan);
                AddOrderInfoRow(worksheet, ref currentRow, "Số điện thoại:", order.SDT);
                AddOrderInfoRow(worksheet, ref currentRow, "Địa chỉ:", order.DiaChi);
                AddOrderInfoRow(worksheet, ref currentRow, "Email:", order.Email);
                AddOrderInfoRow(worksheet, ref currentRow, "Tin nhắn:", order.TinNhan);
                AddOrderInfoRow(worksheet, ref currentRow, "Tình trạng thanh toán:", order.TinhTrangThanhToan);
                AddOrderInfoRow(worksheet, ref currentRow, "Phương thức thanh toán:", order.PhuongThucThanhToan);
                AddOrderInfoRow(worksheet, ref currentRow, "Trạng thái:", order.Status);

                currentRow += 2;

                // Products Table Header
                var headerRow = currentRow;
                worksheet.Cells[headerRow, 1].Value = "Mã SP";
                worksheet.Cells[headerRow, 2].Value = "Tên sản phẩm";
                worksheet.Cells[headerRow, 3].Value = "Số lượng";
                worksheet.Cells[headerRow, 4].Value = "Size";
                worksheet.Cells[headerRow, 5].Value = "Đơn giá";
                worksheet.Cells[headerRow, 6].Value = "Thành tiền";

                // Apply header style
                worksheet.Cells[headerRow, 1, headerRow, 6].StyleName = "HeaderStyle";

                // Add products
                currentRow++;
                foreach (var product in order.SanPhams)
                {
                    worksheet.Cells[currentRow, 1].Value = product.IdSanPham;
                    worksheet.Cells[currentRow, 2].Value = product.TenSP;
                    worksheet.Cells[currentRow, 3].Value = product.SoLuong;
                    worksheet.Cells[currentRow, 4].Value = product.LoaiSize;
                    worksheet.Cells[currentRow, 5].Value = product.GiaSanPham;
                    worksheet.Cells[currentRow, 6].Value = product.GiaSanPham * product.SoLuong;

                    // Format currency cells
                    worksheet.Cells[currentRow, 5, currentRow, 6].Style.Numberformat.Format = "#,##0";

                    currentRow++;
                }

                // Add total
                worksheet.Cells[currentRow, 5].Value = "Tổng tiền:";
                worksheet.Cells[currentRow, 5].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 6].Value = order.TongTien;
                worksheet.Cells[currentRow, 6].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Add borders to the product table
                var tableRange = worksheet.Cells[headerRow, 1, currentRow, 6];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Save the file
                package.SaveAs(new FileInfo(outputPath));
            }

            return File(outputPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DonHang-{CodeOrder}.xlsx");
        }

        private void AddOrderInfoRow(ExcelWorksheet worksheet, ref int currentRow, string label, string value)
        {
            worksheet.Cells[currentRow, 1].Value = label;
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 2].Value = value;
            worksheet.Cells[currentRow, 2, currentRow, 3].Merge = true;
            currentRow++;
        }

        public ActionResult Done(string id)
        {
            var orderDetail = _data.ChiTietDonHang.Find(id);

            if (orderDetail != null)
            {

                if (orderDetail.Status == "Ðã xác nh?n")
                {
                    orderDetail.Status = "Chờ xác nhận";
                    TempData["Message"] = "Đơn hàng chờ xác nhận";
                }
                else
                {
                    orderDetail.Status = "Đã xác nhận";
                    TempData["Message"] = "Đơn hàng đã xác nhận";

                }
                _data.SaveChanges();
                return RedirectToAction("Index");

            }

            TempData["Message"] = "Đơn hàng không tồn tại.";
            return RedirectToAction("Index");
        }

        public ActionResult HuyDon(string idChiTietDonHang)
        {
            // Lấy đơn hàng theo id
            var chiTietDonHangs = _data.ChiTietDonHang.SingleOrDefault(p => p.IdChiTietDonHang == idChiTietDonHang);

            if (chiTietDonHangs == null)
            {
                // Nếu không tìm thấy đơn hàng
                TempData["Message"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index");



                //
            }
            // Lấy CodeOrder từ ChiTietDonHang để tìm đơn hàng liên quan
            string codeOrder = chiTietDonHangs.CodeOrder;
            chiTietDonHangs.Status = "Đơn Đã Hủy";

            // Cập nhật trạng thái của chi tiết đơn hàng
            var DonHang = _data.DonHang.Where(ct => ct.CodeOrder == codeOrder).ToList();

            foreach (var item in DonHang)
            {
                item.Status = "Đơn Đã Hủy";
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _data.SaveChanges();

            // Thông báo thành công
            TempData["Message"] = "Đơn hàng đã được hủy thành công!";
            return RedirectToAction("Index");
        }

    }
}