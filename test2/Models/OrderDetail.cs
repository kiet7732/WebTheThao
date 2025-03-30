using System;
using System.Collections.Generic;

namespace DoAn.Models
{
    public class OrderDetail
    {
        public string CodeOrder { get; set; }
        public DateTime NgayDatHang { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SDT { get; set; }
        public decimal TongTien { get; set; }
        public string TinhTrangThanhToan { get; set; }
        public string Status { get; set; }
        public string IdChiTietDonHang { get; set; }
        public string TinNhan { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public List<ProductDetail> SanPhams { get; set; } = new List<ProductDetail>();
    }

    public class ProductDetail
    {
        public string TenSP { get; set; }
        public string IdSanPham { get; set; }
        public decimal GiaSanPham { get; set; }
        public int SoLuong { get; set; }
        public string LoaiSize { get; set; }
    }

}
