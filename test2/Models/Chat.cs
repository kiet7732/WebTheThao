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
    
    public partial class Chat
    {
        public string IdChat { get; set; } = Guid.NewGuid().ToString();
        public string IdUser { get; set; }
        public string NoiDung { get; set; }
        public Nullable<System.DateTime> NgayGui { get; set; }
        public string Status { get; set; }
        public string HaiLong { get; set; }
        public string VanDe { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
