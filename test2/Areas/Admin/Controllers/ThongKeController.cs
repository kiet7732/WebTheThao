using DoAn.Areas.Admin.Models;
using DoAn.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace DoAn.Areas.Admin.Controllers
{
    public class ThongKeController : Controller
    {
        // GET: Admin/ThongKe
        WebBanHangEntities _data = new WebBanHangEntities();

        public ActionResult DoanhThu(int page = 1)
        {
            // Truy vấn danh sách chi tiết đơn hàng, loại bỏ các đơn hàng có Status == "Đơn Đã Hủy" và nhóm theo ngày
            var thongKeList = _data.ChiTietDonHang
                                    .Where(dh => dh.Status != "Đơn Đã Hủy")  // Lọc các đơn hàng có trạng thái khác "Đơn Đã Hủy"
                                    .GroupBy(dh => DbFunctions.TruncateTime(dh.NgayDatHang))  // Nhóm theo ngày
                                    .Select(group => new ThongKeModelView
                                    {
                                        DoanhThuNgay = (DateTime)group.Key,  // Ngày của nhóm
                                        DoanhThu = group.Sum(dh => dh.TongTien)  // Tính tổng doanh thu cho mỗi ngày
                                    })
                                    .OrderBy(group => group.DoanhThuNgay)  // Sắp xếp theo ngày
                                    .ToList();

            // Tính tổng doanh thu của tất cả các ngày
            decimal? tongDoanhThu = 0;
            foreach (var item in thongKeList)
            {
                tongDoanhThu += item.DoanhThu;  // Cộng dồn doanh thu
            }

            // Truyền tổng doanh thu vào ViewBag
            ViewBag.tongThu = tongDoanhThu;

            // Đưa kết quả vào View và phân trang
            return View(thongKeList.ToPagedList(page, 4));
        }



        public ActionResult ExportDoanhThuToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Lấy dữ liệu thống kê
            var thongKeList = _data.ChiTietDonHang
                .Where(dh => dh.Status != "Đơn Đã Hủy")
                .GroupBy(dh => DbFunctions.TruncateTime(dh.NgayDatHang))
                .Select(group => new ThongKeModelView
                {
                    DoanhThuNgay = (DateTime)group.Key,
                    DoanhThu = group.Sum(dh => dh.TongTien)
                })
                .OrderBy(group => group.DoanhThuNgay)
                .ToList();

            decimal? tongDoanhThu = thongKeList.Sum(item => item.DoanhThu);

            string templatePath = Server.MapPath("~/Areas/Admin/Excel/TemplateDoanhThu.xlsx");
            string outputPath = Server.MapPath($"~/Areas/Admin/Excel/User/BaoCaoDoanhThu-{DateTime.Now:yyyyMMddHHmmss}.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return new HttpStatusCodeResult(404, "File template không tồn tại.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Định nghĩa styles
                var headerStyle = package.Workbook.Styles.CreateNamedStyle("HeaderStyle");
                headerStyle.Style.Font.Bold = true;
                headerStyle.Style.Font.Size = 12;
                headerStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerStyle.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(48, 84, 150));
                headerStyle.Style.Font.Color.SetColor(Color.White);
                headerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerStyle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var titleStyle = package.Workbook.Styles.CreateNamedStyle("TitleStyle");
                titleStyle.Style.Font.Bold = true;
                titleStyle.Style.Font.Size = 16;
                titleStyle.Style.Font.Color.SetColor(Color.FromArgb(44, 62, 80));
                titleStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var totalRowStyle = package.Workbook.Styles.CreateNamedStyle("TotalRowStyle");
                totalRowStyle.Style.Font.Bold = true;
                totalRowStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
                totalRowStyle.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(241, 196, 15));
                totalRowStyle.Style.Font.Color.SetColor(Color.FromArgb(44, 62, 80));

                // Logo và Tiêu đề
                worksheet.Cells["A1:C1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO DOANH THU";
                worksheet.Cells["A1"].StyleName = "TitleStyle";

                // Thông tin thời gian báo cáo
                worksheet.Cells["A2:C2"].Merge = true;
                worksheet.Cells["A2"].Value = $"Thời gian xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                // Header của bảng
                int headerRow = 4;
                worksheet.Cells[headerRow, 1].Value = "STT";
                worksheet.Cells[headerRow, 2].Value = "Ngày";
                worksheet.Cells[headerRow, 3].Value = "Doanh Thu (VNĐ)";
                worksheet.Cells[headerRow, 1, headerRow, 3].StyleName = "HeaderStyle";

                // Set column widths
                worksheet.Column(1).Width = 8;  // STT
                worksheet.Column(2).Width = 20; // Ngày
                worksheet.Column(3).Width = 25; // Doanh Thu

                // Đổ dữ liệu
                int currentRow = headerRow + 1;
                int stt = 1;
                foreach (var item in thongKeList)
                {
                    worksheet.Cells[currentRow, 1].Value = stt++;
                    worksheet.Cells[currentRow, 2].Value = item.DoanhThuNgay?.ToString("dd/MM/yyyy");
                    worksheet.Cells[currentRow, 3].Value = item.DoanhThu;

                    // Format số tiền
                    worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "#,##0";

                    // Alternate row colors
                    if (currentRow % 2 == 0)
                    {
                        worksheet.Cells[currentRow, 1, currentRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[currentRow, 1, currentRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 244, 244));
                    }

                    // Căn giữa cột STT và Ngày
                    worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    currentRow++;
                }

                // Thêm dòng tổng
                int totalRow = currentRow + 1;
                worksheet.Cells[totalRow, 1, totalRow, 2].Merge = true;
                worksheet.Cells[totalRow, 1].Value = "TỔNG DOANH THU";
                worksheet.Cells[totalRow, 3].Value = tongDoanhThu;
                worksheet.Cells[totalRow, 1, totalRow, 3].StyleName = "TotalRowStyle";
                worksheet.Cells[totalRow, 3].Style.Numberformat.Format = "#,##0";

                // Thêm borders cho toàn bộ bảng
                var tableRange = worksheet.Cells[headerRow, 1, totalRow, 3];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Thêm thông tin thống kê
                currentRow = totalRow + 3;
                worksheet.Cells[currentRow, 1, currentRow, 3].Merge = true;
                worksheet.Cells[currentRow, 1].Value = "THÔNG TIN THỐNG KÊ";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                currentRow++;
                worksheet.Cells[currentRow, 1, currentRow, 2].Merge = true;
                worksheet.Cells[currentRow, 1].Value = "Tổng số ngày thống kê:";
                worksheet.Cells[currentRow, 3].Value = thongKeList.Count;

                currentRow++;
                worksheet.Cells[currentRow, 1, currentRow, 2].Merge = true;
                worksheet.Cells[currentRow, 1].Value = "Doanh thu trung bình/ngày:";
                worksheet.Cells[currentRow, 3].Value = thongKeList.Count > 0 ? tongDoanhThu / thongKeList.Count : 0;
                worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "#,##0";

                // Thêm footer
                currentRow += 2;
                worksheet.Cells[currentRow, 1, currentRow, 3].Merge = true;
                worksheet.Cells[currentRow, 1].Value = "© " + DateTime.Now.Year + " - Báo cáo được tạo tự động từ hệ thống";
                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[currentRow, 1].Style.Font.Italic = true;
                worksheet.Cells[currentRow, 1].Style.Font.Color.SetColor(Color.Gray);

                package.SaveAs(new FileInfo(outputPath));
            }

            return File(outputPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BaoCaoDoanhThu-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

    }
}