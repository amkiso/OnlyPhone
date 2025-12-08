using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class FeedbackViewModel
    {

        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; } 
        public string Message { get; set; }

    }
}