using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace DoAn.Areas.Admin.Models
{

    public class ThongKeModelView
    {
        public DateTime? DoanhThuNgay { set; get; }
        public decimal? DoanhThu { set; get; }
    }
}

