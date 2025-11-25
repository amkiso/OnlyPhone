using System;

namespace OnlyPhone.Areas.Admin.Data
{
    public class UserListViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }

        // Phân quyền & Trạng thái
        public string Role { get; set; }      // Admin, Staff, Customer
        public bool IsLocked { get; set; }    // Cột [Locked] trong bảng Users
        public bool IsOnline { get; set; }    // Cột [user_status] trong bảng User_detail

        // Thời gian
        public DateTime? LastActive { get; set; } // Cột [last_change]
        public DateTime CreatedDate { get; set; } // Cột [date_create]
    }
}