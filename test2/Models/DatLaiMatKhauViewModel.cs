using System.ComponentModel.DataAnnotations;
namespace DoAn.Models
{
    public class DatLaiMatKhauViewModel
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }

        public string Email { get; set; }
    }
}