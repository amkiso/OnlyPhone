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
        public string Province { get; set; }
        public string Ward { get; set; }
        public string HomeNumber { get; set; }
        public string FullAddress
        {
            get;set;
        }
        public string Avatar { get; set; }

        // Phân quyền & Trạng thái
        public string Role { get; set; }     
        public bool IsLocked { get; set; }   
        public bool IsOnline { get; set; }    

        // Thời gian
        public DateTime? LastActive { get; set; } 
        public DateTime CreatedDate { get; set; } 
    }
}