
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

using VocabMaster.Models;
using VocabMaster.Helpers;

namespace VocabMaster.Controllers
{
    public class AccountController : Controller
    {
        private sealed class PasswordResetRequest
        {
            public string Email { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        private static readonly Dictionary<string, PasswordResetRequest> ResetTokens =
            new Dictionary<string, PasswordResetRequest>(StringComparer.Ordinal);

        private static readonly object ResetLock = new object();

        // =========================
        // LOGIN - GET
        // =========================

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View("~/Views/ClientViews/Account/Login.cshtml");
        }

        // =========================
        // LOGIN - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            string email,
            string password,
            bool rememberMe = false,
            string returnUrl = null)
        {
            email = (email ?? "").Trim();

            ViewBag.Email = email;
            ViewBag.RememberMe = rememberMe;
            ViewBag.ReturnUrl = returnUrl;

            if (String.IsNullOrWhiteSpace(email) ||
                String.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                return View("~/Views/ClientViews/Account/Login.cshtml");
            }

            using (var db = new VocabMasterDBEntities())
            {
                User user = db.Users.FirstOrDefault(
                    x => x.Email == email
                );

                if (user == null ||
                    !user.IsActive ||
                    !PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    ViewBag.Error =
                        "Email hoặc mật khẩu chưa đúng, hoặc tài khoản đã bị khóa.";

                    return View("~/Views/ClientViews/Account/Login.cshtml");
                }

                // Update last login time.
                user.LastLoginAt = DateTime.Now;
                db.SaveChanges();

                // Save login information to Session.
                Session["UserId"] = user.UserId;
                Session["UserEmail"] = user.Email;
                Session["FullName"] =
                    !String.IsNullOrWhiteSpace(user.DisplayName)
                        ? user.DisplayName
                        : user.Username;

                Session["Role"] = user.Role;
                Session["RememberMe"] = rememberMe;
            }

            // Redirect to a valid local return URL if supplied.
            if (!String.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Admin goes to Admin Dashboard.
            if (String.Equals(
                Convert.ToString(Session["Role"]),
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Normal user goes to Client Dashboard.
            return RedirectToAction("Index", "Dashboard");
        }

        // =========================
        // REGISTER - GET
        // =========================

        [HttpGet]
        public ActionResult Register()
        {
            return View("~/Views/ClientViews/Account/Register.cshtml");
        }

        // =========================
        // REGISTER - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            string fullName,
            string email,
            string password,
            string confirmPassword,
            bool agreeTerms = false)
        {
            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();

            ViewBag.FullName = fullName;
            ViewBag.Email = email;

            if (String.IsNullOrWhiteSpace(fullName) ||
                String.IsNullOrWhiteSpace(email) ||
                String.IsNullOrEmpty(password))
            {
                ViewBag.Error =
                    "Vui lòng điền đầy đủ họ tên, email và mật khẩu.";
            }
            else if (fullName.Length > 100)
            {
                ViewBag.Error = "Họ tên không được vượt quá 100 ký tự.";
            }
            else if (email.Length > 255 ||
                     !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                         .IsValid(email))
            {
                ViewBag.Error = "Email chưa đúng định dạng.";
            }
            else if (password.Length < 8 ||
                     !password.Any(Char.IsLetter) ||
                     !password.Any(Char.IsDigit))
            {
                ViewBag.Error =
                    "Mật khẩu cần ít nhất 8 ký tự, gồm cả chữ và số.";
            }
            else if (!String.Equals(
                password,
                confirmPassword,
                StringComparison.Ordinal))
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            }
            else if (!agreeTerms)
            {
                ViewBag.Error =
                    "Bạn cần đồng ý với Điều khoản sử dụng và Chính sách bảo mật.";
            }
            else
            {
                using (var db = new VocabMasterDBEntities())
                {
                    bool emailExists = db.Users.Any(
                        x => x.Email == email
                    );

                    if (emailExists)
                    {
                        ViewBag.Error =
                            "Email này đã được đăng ký. Hãy đăng nhập.";
                    }
                    else
                    {
                        string username = CreateUniqueUsername(db, email);

                        var user = new User
                        {
                            Username = username,
                            Email = email,
                            PasswordHash = PasswordHasher.HashPassword(password),
                            DisplayName = fullName,
                            AvatarUrl = null,
                            Role = "User",
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            LastLoginAt = DateTime.Now
                        };

                        db.Users.Add(user);
                        db.SaveChanges();

                        Session["UserId"] = user.UserId;
                        Session["UserEmail"] = user.Email;
                        Session["FullName"] = user.DisplayName;
                        Session["Role"] = user.Role;
                        Session["RememberMe"] = false;

                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }

            return View("~/Views/ClientViews/Account/Register.cshtml");
        }

        // =========================
        // CREATE UNIQUE USERNAME
        // =========================

        private static string CreateUniqueUsername(
            VocabMasterDBEntities db,
            string email)
        {
            string emailPrefix = email.Split('@')[0];

            string baseName = new string(
                emailPrefix
                    .Where(Char.IsLetterOrDigit)
                    .ToArray()
            );

            if (String.IsNullOrWhiteSpace(baseName))
            {
                baseName = "user";
            }

            if (baseName.Length > 40)
            {
                baseName = baseName.Substring(0, 40);
            }

            string username = baseName;
            int number = 1;

            while (db.Users.Any(x => x.Username == username))
            {
                string suffix = number.ToString();
                int maxBaseLength = 50 - suffix.Length;

                username = baseName.Substring(
                    0,
                    Math.Min(baseName.Length, maxBaseLength)
                ) + suffix;

                number++;
            }

            return username;
        }

        // =========================
        // FORGOT PASSWORD - GET
        // =========================

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("~/Views/ClientViews/Account/ForgotPassword.cshtml");
        }

        // =========================
        // FORGOT PASSWORD - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            email = (email ?? "").Trim();
            ViewBag.Email = email;

            if (String.IsNullOrWhiteSpace(email) ||
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(email))
            {
                ViewBag.Error = "Vui lòng nhập địa chỉ email hợp lệ.";

                return View("~/Views/ClientViews/Account/ForgotPassword.cshtml");
            }

            bool accountExists;

            using (var db = new VocabMasterDBEntities())
            {
                accountExists = db.Users.Any(
                    x => x.Email == email && x.IsActive
                );
            }

            // Do not reveal whether an email is registered.
            if (!accountExists)
            {
                ViewBag.Message =
                    "Nếu email thuộc tài khoản đã đăng ký, hướng dẫn đặt lại mật khẩu " +
                    "sẽ được gửi đến hộp thư của bạn. Hãy kiểm tra cả Spam.";

                return View("~/Views/ClientViews/Account/ForgotPassword.cshtml");
            }

            string token = CreateResetToken();

            lock (ResetLock)
            {
                List<string> oldTokens = ResetTokens
                    .Where(x =>
                        x.Value.ExpiresAt <= DateTime.UtcNow ||
                        String.Equals(
                            x.Value.Email,
                            email,
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
                    "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư " +
                    "đến và Spam. Liên kết có hiệu lực trong 30 phút.";
            }
            catch (Exception)
            {
                lock (ResetLock)
                {
                    ResetTokens.Remove(token);
                }

                ViewBag.Error =
                    "Không thể gửi email lúc này. Hãy kiểm tra cấu hình Gmail SMTP " +
                    "trong Web.config, sau đó thử lại.";
            }

            return View("~/Views/ClientViews/Account/ForgotPassword.cshtml");
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

                return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
            }

            ViewBag.Token = token;
            ViewBag.Email = request.Email;

            return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
        }

        // =========================
        // RESET PASSWORD - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(
            string token,
            string password,
            string confirmPassword)
        {
            PasswordResetRequest request = GetValidResetRequest(token);

            if (request == null)
            {
                ViewBag.Error =
                    "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. " +
                    "Vui lòng yêu cầu gửi email mới.";

                return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
            }

            ViewBag.Token = token;
            ViewBag.Email = request.Email;

            if (String.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu mới.";

                return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
            }

            if (password.Length < 8 ||
                !password.Any(Char.IsLetter) ||
                !password.Any(Char.IsDigit))
            {
                ViewBag.Error =
                    "Mật khẩu mới phải có ít nhất 8 ký tự, gồm cả chữ và số.";

                return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
            }

            if (String.IsNullOrEmpty(confirmPassword) ||
                !String.Equals(
                    password,
                    confirmPassword,
                    StringComparison.Ordinal))
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";

                return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
            }

            using (var db = new VocabMasterDBEntities())
            {
                User user = db.Users.FirstOrDefault(
                    x => x.Email == request.Email && x.IsActive
                );

                if (user == null)
                {
                    ViewBag.Error =
                        "Không tìm thấy tài khoản hợp lệ để cập nhật mật khẩu.";

                    return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
                }

                user.PasswordHash = PasswordHasher.HashPassword(password);
                db.SaveChanges();
            }

            // A reset link can only be used once.
            lock (ResetLock)
            {
                ResetTokens.Remove(token);
            }

            ViewBag.Success =
                "Mật khẩu đã được thay đổi thành công. Bạn có thể đăng nhập bằng mật khẩu mới.";

            return View("~/Views/ClientViews/Account/ResetPassword.cshtml");
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
            string senderEmail =
                ConfigurationManager.AppSettings["SmtpUser"];

            string appPassword =
                ConfigurationManager.AppSettings["SmtpAppPassword"];

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
                    "<p><a href='" +
                    HttpUtility.HtmlAttributeEncode(resetUrl) +
                    "' style='display:inline-block;padding:12px 20px;background:#2454d9;color:#fff;text-decoration:none;border-radius:8px'>" +
                    "Tạo mật khẩu mới</a></p>" +
                    "<p>Liên kết chỉ có hiệu lực trong 30 phút và chỉ sử dụng được một lần.</p>" +
                    "<p>Nếu bạn không yêu cầu thao tác này, hãy bỏ qua email.</p>" +
                    "</div>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials =
                        new NetworkCredential(senderEmail, appPassword);

                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
        }



        


        public ActionResult Logout()
        {
            Session.Remove("UserId");
            Session.Remove("UserEmail");
            Session.Remove("FullName");
            Session.Remove("Role");
            Session.Remove("RememberMe");

            return RedirectToAction("Login");
        }
    }
}
