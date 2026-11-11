using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace VocabMaster.Controllers
{
    public class AccountController : Controller
    {
        // Demo account store for coursework only.
        // Accounts are stored in memory, not in SQL Server.
        internal static readonly Dictionary<string, string> DemoUsers =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "minhanh@example.com", "Vocab1234" }
            };

        private sealed class PasswordResetRequest
        {
            public string Email { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        private static readonly Dictionary<string, PasswordResetRequest> ResetTokens =
            new Dictionary<string, PasswordResetRequest>(StringComparer.Ordinal);

        private static readonly object ResetLock = new object();

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/ClientViews/Account/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password,
            bool rememberMe = false, string returnUrl = null)
        {
            email = (email ?? "").Trim();

            string savedPassword;
            if (email.Length == 0 ||
                password == null ||
                !DemoUsers.TryGetValue(email, out savedPassword) ||
                !String.Equals(savedPassword, password, StringComparison.Ordinal))
            {
                ViewBag.Error = "Email hoặc mật khẩu chưa đúng. Hãy kiểm tra tài khoản của bạn.";
                ViewBag.Email = email;
                ViewBag.RememberMe = rememberMe;
                ViewBag.ReturnUrl = returnUrl;
                return View("~/Views/ClientViews/Account/Login.cshtml");
            }

            Session["UserEmail"] = email;

            if (String.Equals(email, "minhanh@example.com",
                StringComparison.OrdinalIgnoreCase))
            {
                Session["FullName"] = "Nguyễn Minh Anh";
            }

            Session["RememberMe"] = rememberMe;

            if (!String.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // =========================
        // REGISTER
        // =========================

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullName, string email,
            string password, string confirmPassword, bool agreeTerms = false)
        {
            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();

            ViewBag.FullName = fullName;
            ViewBag.Email = email;

            if (fullName.Length == 0 || email.Length == 0 || password == null)
            {
                ViewBag.Error = "Vui lòng điền đầy đủ họ tên, email và mật khẩu.";
            }
            else if (!email.Contains("@") || !email.Contains("."))
            {
                ViewBag.Error = "Email chưa đúng định dạng.";
            }
            else if (password.Length < 8 ||
                     !password.Any(Char.IsLetter) ||
                     !password.Any(Char.IsDigit))
            {
                ViewBag.Error = "Mật khẩu cần ít nhất 8 ký tự, gồm cả chữ và số.";
            }
            else if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            }
            else if (!agreeTerms)
            {
                ViewBag.Error = "Bạn cần đồng ý với Điều khoản sử dụng và Chính sách bảo mật.";
            }
            else if (DemoUsers.ContainsKey(email))
            {
                ViewBag.Error = "Email này đã được đăng ký. Hãy đăng nhập.";
            }
            else
            {
                DemoUsers[email] = password;
                Session["UserEmail"] = email;
                Session["FullName"] = fullName;
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // =========================
        // FORGOT PASSWORD
        // =========================

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            email = (email ?? "").Trim();
            ViewBag.Email = email;

            if (String.IsNullOrWhiteSpace(email) ||
                !email.Contains("@") ||
                !email.Contains("."))
            {
                ViewBag.Error = "Vui lòng nhập địa chỉ email hợp lệ.";
                return View();
            }

            // Do not reveal whether an email is registered.
            if (!DemoUsers.ContainsKey(email))
            {
                ViewBag.Message =
                    "Nếu email thuộc tài khoản đã đăng ký, hướng dẫn đặt lại mật khẩu " +
                    "sẽ được gửi đến hộp thư của bạn. Nếu chưa nhận được, hãy kiểm tra " +
                    "Spam hoặc xác nhận email đã đăng ký.";
                return View();
            }

            string token = CreateResetToken();

            lock (ResetLock)
            {
                // Remove expired tokens and previous tokens for this email.
                List<string> oldTokens = ResetTokens
                    .Where(x => x.Value.ExpiresAt <= DateTime.UtcNow ||
                                String.Equals(x.Value.Email, email,
                                    StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Key)
                    .ToList();

                foreach (string oldToken in oldTokens)
                {
                    ResetTokens.Remove(oldToken);
                }

                ResetTokens[token] = new PasswordResetRequest
                {
                    Email = email,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };
            }

            try
            {
                string resetUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = token },
                    Request.Url.Scheme
                );

                SendResetEmail(email, resetUrl);

                ViewBag.Message =
                    "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư đến " +
                    "và Spam. Liên kết có hiệu lực trong 30 phút.";
            }
            catch (Exception)
            {
                lock (ResetLock)
                {
                    ResetTokens.Remove(token);
                }

                ViewBag.Error =
                    "Không thể gửi email lúc này. Hãy kiểm tra cấu hình Gmail SMTP " +
                    "trong Web.config (Gmail và Mật khẩu ứng dụng), sau đó thử lại.";
            }

            return View();
        }

        // =========================
        // RESET PASSWORD - GET
        // =========================

        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            PasswordResetRequest request = GetValidResetRequest(token);

            if (request == null)
            {
                ViewBag.Error =
                    "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. " +
                    "Vui lòng yêu cầu gửi email mới.";
                return View();
            }

            ViewBag.Token = token;
            ViewBag.Email = request.Email;

            return View();
        }

        // =========================
        // RESET PASSWORD - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string token, string password, string confirmPassword)
        {
            PasswordResetRequest request = GetValidResetRequest(token);

            if (request == null)
            {
                ViewBag.Error =
                    "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. " +
                    "Vui lòng yêu cầu gửi email mới.";
                return View();
            }

            // Preserve token and email when validation fails, so the user can retry.
            ViewBag.Token = token;
            ViewBag.Email = request.Email;

            // Validate the new password.
            if (String.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu mới.";
                return View();
            }

            if (password.Length < 8 ||
                !password.Any(Char.IsLetter) ||
                !password.Any(Char.IsDigit))
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 8 ký tự, gồm cả chữ và số.";
                return View();
            }

            // IMPORTANT: compare the two submitted fields exactly.
            // Example: cachua123 and cachua1234 MUST be rejected.
            if (String.IsNullOrEmpty(confirmPassword) ||
                !String.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp. Vui lòng nhập lại.";
                return View();
            }

            // Save only after BOTH password checks have passed.
            DemoUsers[request.Email] = password;

            // A reset link can only be used once.
            lock (ResetLock)
            {
                ResetTokens.Remove(token);
            }

            ViewBag.Success =
                "Mật khẩu đã được thay đổi thành công. Bạn có thể đăng nhập bằng mật khẩu mới.";

            return View();
        }

        // =========================
        // RESET TOKEN HELPERS
        // =========================

        private static PasswordResetRequest GetValidResetRequest(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            lock (ResetLock)
            {
                PasswordResetRequest request;

                if (!ResetTokens.TryGetValue(token, out request))
                {
                    return null;
                }

                if (request.ExpiresAt <= DateTime.UtcNow)
                {
                    ResetTokens.Remove(token);
                    return null;
                }

                return request;
            }
        }

        private static string CreateResetToken()
        {
            byte[] bytes = new byte[32];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        // =========================
        // SEND RESET EMAIL
        // =========================

        private static void SendResetEmail(string recipient, string resetUrl)
        {
            string senderEmail = ConfigurationManager.AppSettings["SmtpUser"];
            string appPassword = ConfigurationManager.AppSettings["SmtpAppPassword"];

            if (String.IsNullOrWhiteSpace(senderEmail) ||
                String.IsNullOrWhiteSpace(appPassword) ||
                senderEmail == "your-gmail@gmail.com" ||
                appPassword == "YOUR_16_CHARACTER_APP_PASSWORD")
            {
                throw new ConfigurationErrorsException(
                    "Gmail SMTP has not been configured."
                );
            }

            using (MailMessage message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, "VocabMaster");
                message.To.Add(recipient);
                message.Subject = "Đặt lại mật khẩu VocabMaster";
                message.IsBodyHtml = true;

                message.Body =
                    "<div style='font-family:Arial,sans-serif;line-height:1.6;color:#172033'>" +
                    "<h2>Yêu cầu đặt lại mật khẩu</h2>" +
                    "<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản VocabMaster của bạn.</p>" +
                    "<p><a href='" + HttpUtility.HtmlAttributeEncode(resetUrl) +
                    "' style='display:inline-block;padding:12px 20px;background:#2454d9;color:#fff;text-decoration:none;border-radius:8px'>" +
                    "Tạo mật khẩu mới</a></p>" +
                    "<p>Liên kết chỉ có hiệu lực trong 30 phút và chỉ sử dụng được một lần.</p>" +
                    "<p>Nếu bạn không yêu cầu thao tác này, hãy bỏ qua email.</p>" +
                    "</div>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
        }

        // =========================
        // LOGOUT
        // =========================

        public ActionResult Logout()
        {
            Session.Remove("UserEmail");
            Session.Remove("FullName");
            Session.Remove("RememberMe");

            return RedirectToAction("Login");
        }
    }
}
