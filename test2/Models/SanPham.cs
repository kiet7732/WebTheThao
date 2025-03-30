//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DoAn.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SanPham
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SanPham()
        {
            this.DonHang = new HashSet<DonHang>();
            this.GioHang = new HashSet<GioHang>();
        }
    
        public string IdSanPham { get; set; } = Guid.NewGuid().ToString();
        public string LoaiSanPham { get; set; }
        public string HangSanPham { get; set; }
        public string MoTaSanPham { get; set; }
        public Nullable<System.DateTime> NgayRaMat { get; set; }
        public string TenSanPham { get; set; }
        public string LinkHinhAnh { get; set; }
        public string LinkSanPham { get; set; }
        public decimal GiaSanPham { get; set; }
        public int SoLuongTon { get; set; }
        public string IdSizeSp { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DonHang> DonHang { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GioHang> GioHang { get; set; }
        public virtual HangSanPham HangSanPham1 { get; set; }
        public virtual KichCo KichCo { get; set; }
        public virtual LoaiSanPham LoaiSanPham1 { get; set; }
    }
}
