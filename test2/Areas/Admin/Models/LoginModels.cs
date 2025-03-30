using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace DoAn.Areas.Admin.Models
{
    // GET: Admin/LoginModels
    public class LoginModel
    {
        [Required(ErrorMessage = "Mời nhập tài khoản")]
        [Display(Name = "Tài Khoản")]
        public string userName { set; get; }
        [Required(ErrorMessage = "Mời nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        public string passWord { set; get; }

        public bool Remember { set; get; }
    }
}