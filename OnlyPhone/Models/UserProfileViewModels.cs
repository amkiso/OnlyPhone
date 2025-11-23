using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace OnlyPhone.Models
{
// View models
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ (vd: 0912345678)")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public string Province { get; set; }


        [Display(Name = "Phường/Xã")]
        public string Ward { get; set; }

        [Display(Name = "Địa chỉ chi tiết")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string AddressDetail { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string AvatarUrl { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Lần cập nhật cuối")]
        public DateTime? LastModified { get; set; }

        public bool IsActive { get; set; }

        public string RoleType { get; set; }

        // Full address
        public string FullAddress
        {
            get
            {
                var parts = new[]
                {
                    AddressDetail,
                    Ward,
                    Province
                };

                return string.Join(", ", Array.FindAll(parts, s => !string.IsNullOrWhiteSpace(s)));
            }
        }

        // Avatar path for display
        public string AvatarPath => $"~/Content/Pic/Avartar/{AvatarUrl ?? "noAvt.jpg"}";
    }

//Yêu cầu Sửa thông tin hồ sơ cá nhân
    public class EditProfileRequest
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string AddressDetail { get; set; }

        // File upload
        public HttpPostedFileBase AvatarFile { get; set; }
    }
    //Request đổi mật khẩu
    public class ChangePasswordRequest
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateProfileResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AvatarUrl { get; set; }
        public string ErrorCode { get; set; }
    }
    public class AvatarUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
    }
}