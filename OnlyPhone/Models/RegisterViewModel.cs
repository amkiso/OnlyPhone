using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class RegisterViewModel
    {
        
        
            [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
            [StringLength(20, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có từ 3-20 ký tự")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [StringLength(30)]
            public string Email { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
            public string ConfirmPassword { get; set; }

            public string OtpCode { get; set; }
        

        public class OTPModel
        {
            public string Email { get; set; }
            public string OtpCode { get; set; }
            public DateTime ExpiryTime { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}