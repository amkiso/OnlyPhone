using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace OnlyPhone.Models
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService()
        {
            // Đọc config từ Web.config
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            _fromEmail = ConfigurationManager.AppSettings["FromEmail"];
            _fromName = ConfigurationManager.AppSettings["FromName"] ?? "TechStore";
        }

        public bool SendOTPEmail(string toEmail, string otpCode, string username)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, _fromName),
                        Subject = "Mã OTP xác thực đăng ký - OnlyPhone",
                        IsBodyHtml = true,
                        Body = GetOTPEmailTemplate(username, otpCode)
                    };

                    mailMessage.To.Add(toEmail);

                    client.Send(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        private string GetOTPEmailTemplate(string username, string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{font - family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{max - width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
        .header {{background: linear-gradient(135deg, #667eea 0%, #00FFFF 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{margin: 0; font-size: 28px; }}
        .content {{padding: 40px 30px; text-align: center; }}
        .otp-box {{background: #f8f9fa; border: 2px dashed #667eea; border-radius: 10px; padding: 30px; margin: 30px 0; }}
        .otp-code {{font - size: 42px; font-weight: bold; color: #6a75a6; letter-spacing: 10px; margin: 20px 0; }}
        .info-text {{color: #666; font-size: 14px; line-height: 1.6; margin: 20px 0; }}
        .warning {{background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; text-align: left; }}
        .footer {{background: #f8f9fa; padding: 20px; text-align: center; color: #666; font-size: 12px; }}
        .logo {{font - size: 24px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>OnlyPhone</div>
            <h1>Xác thực đăng ký tài khoản</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{username}</strong>,</p>
            <p class='info-text'>Cảm ơn bạn đã đăng ký tài khoản tại TechStore. Vui lòng sử dụng mã OTP bên dưới để hoàn tất quá trình đăng ký:</p>
            
            <div class='otp-box'>
                <div style='color: #333; font-size: 16px; margin-bottom: 10px;'>Mã OTP của bạn</div>
                <div class='otp-code'>{otpCode}</div>
                <div style='color: #999; font-size: 14px; margin-top: 10px;'>Mã có hiệu lực trong 5 phút</div>
            </div>

            <div class='warning'>
                <strong>⚠️ Lưu ý bảo mật:</strong><br>
                - Không chia sẻ mã OTP với bất kỳ ai<br>
                - TechStore không bao giờ yêu cầu mã OTP qua điện thoại<br>
                - Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email
            </div>

            <p class='info-text'>Nếu bạn cần hỗ trợ, vui lòng liên hệ với chúng tôi qua email: thangcuoi1984a@gmail.com</p>
        </div>
        <div class='footer'>
            <p>© 2025 onlyPhone. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        public bool SendWelcomeEmail(string toEmail, string username)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, _fromName),
                        Subject = "Chào mừng đến với OnlyPhone!",
                        IsBodyHtml = true,
                        Body = GetWelcomeEmailTemplate(username)
                    };

                    mailMessage.To.Add(toEmail);

                    client.Send(mailMessage);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetWelcomeEmailTemplate(string username)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{font - family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{max - width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
        .header {{background: linear-gradient(135deg, #667eea 0%, #00FFFF 100%); color: white; padding: 40px; text-align: center; }}
        .content {{padding: 40px 30px; }}
        .button {{display: inline-block; background: linear-gradient(135deg, #667eea 0%, #00FFFF 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; margin: 20px 0; font-weight: bold; }}
        .features {{display: flex; flex-wrap: wrap; gap: 20px; margin: 30px 0;  }}
        .feature {{flex: 1; min-width: 200px; text-align: center; padding: 20px;border: 1px solid rgb(68, 142, 196); background: #f8f9fa; border-radius: 8px; margin-bottom: 1rem;}}
        .footer {{background: #f8f9fa; padding: 20px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Chào mừng đến với Onlyphone!</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {username}!</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại OnlyPhone. Chúng tôi rất vui được đồng hành cùng bạn!</p>
            
            <div style='text-align: center;'>
                <a href='#' class='button'>Bắt đầu mua sắm</a>
            </div>

           
                <div class='feature'>
                    <div style='font-size: 32px;'>🚚</div>
                    <h3>Giao hàng nhanh trên toàn quốc</h3>
                    <p>Bạn đặt, chúng tôi giao hàng dù bất kể bạn ở đâu, đơn hàng cũng sẽ đến được tay bạn</p>
                </div>
                <div class='feature'>
                    <div style='font-size: 32px;'>💳</div>
                    <h3>Thanh toán đa dạng</h3>
                    <p>Hỗ trợ nhiều hình thức thanh toán</p>
                </div>
                <div class='feature'>
                    <div style='font-size: 32px;'>🎁</div>
                    <h3>Ưu đãi độc quyền</h3>
                    <p>Nhận voucher và khuyến mãi hấp dẫn</p>
                </div>
            

            <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi!</p>
        </div>
        <div class='footer'>
            <p>© 2025 OnlyPhone. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
        }
}