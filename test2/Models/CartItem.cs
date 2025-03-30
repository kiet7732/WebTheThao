using System.Collections.Generic;

namespace DoAn.Models
{
    public class CartItem
    {
        public string IdSanPham { get; set; }
        public string IdGioHang { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string LinkHinhAnh { get; set; }
        public string LoaiSize { get; set; }
        public string IdSizeSp { get; set; }
    }

}