using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using VocabMaster.ViewModels;

namespace VocabMaster.Controllers
{
    public class ProfileController : Controller
    {
        private ProfileViewModel BuildModel()
        {
            string email = Convert.ToString(Session["UserEmail"]);
            string name = Convert.ToString(Session["FullName"]);
            if (String.IsNullOrWhiteSpace(name))
                name = String.IsNullOrWhiteSpace(email) ? "Học viên" : email.Split('@')[0];

            return new ProfileViewModel
            {
                FullName = name,
                Email = email,
                Role = "Sinh viên đại học",
                AvatarUrl = Convert.ToString(Session["AvatarUrl"]),
                DailyGoal = Convert.ToInt32(Session["DailyGoal"] ?? 10),
                ReminderTime = Convert.ToString(Session["ReminderTime"] ?? "20:00"),
                EmailReminder = Session["EmailReminder"] == null || Convert.ToBoolean(Session["EmailReminder"]),
                PronunciationSound = Session["PronunciationSound"] == null || Convert.ToBoolean(Session["PronunciationSound"])
            };
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserEmail"] == null) return RedirectToAction("Login", "Account");
            return View(BuildModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ProfileViewModel model)
        {
            if (Session["UserEmail"] == null) return RedirectToAction("Login", "Account");
            var current = BuildModel();
            model.Email = current.Email;
            model.Role = current.Role;
            model.AvatarUrl = current.AvatarUrl;

            if (String.IsNullOrWhiteSpace(model.FullName))
            {
                model.Error = "Vui lòng nhập họ và tên.";
                return View("Index", model);
            }
            if (model.DailyGoal < 1 || model.DailyGoal > 100)
            {
                model.Error = "Mục tiêu mỗi ngày phải từ 1 đến 100 từ.";
                return View("Index", model);
            }
            if (String.IsNullOrWhiteSpace(model.ReminderTime))
                model.ReminderTime = "20:00";

            Session["FullName"] = model.FullName.Trim();
            Session["DailyGoal"] = model.DailyGoal;
            Session["ReminderTime"] = model.ReminderTime;
            Session["EmailReminder"] = model.EmailReminder;
            Session["PronunciationSound"] = model.PronunciationSound;
            model.Message = "Đã lưu thông tin cá nhân và thói quen học tập.";
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {
            if (Session["UserEmail"] == null) return RedirectToAction("Login", "Account");
            if (avatar == null || avatar.ContentLength == 0)
            {
                TempData["ProfileError"] = "Vui lòng chọn ảnh đại diện.";
                return RedirectToAction("Index");
            }

            string extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                TempData["ProfileError"] = "Ảnh đại diện chỉ chấp nhận JPG, PNG hoặc WebP.";
                return RedirectToAction("Index");
            }
            if (avatar.ContentLength > 2 * 1024 * 1024)
            {
                TempData["ProfileError"] = "Ảnh đại diện không được vượt quá 2 MB.";
                return RedirectToAction("Index");
            }

            string folder = Server.MapPath("~/Uploads/Avatars");
            Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            avatar.SaveAs(Path.Combine(folder, fileName));
            Session["AvatarUrl"] = Url.Content("~/Uploads/Avatars/" + fileName);
            TempData["ProfileMessage"] = "Đã cập nhật ảnh đại diện.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserEmail"] == null) return RedirectToAction("Login", "Account");
            string email = Convert.ToString(Session["UserEmail"]);
            string savedPassword;
            if (!AccountController.DemoUsers.TryGetValue(email, out savedPassword) || savedPassword != currentPassword)
                TempData["PasswordError"] = "Mật khẩu hiện tại chưa chính xác.";
            else if (String.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8 ||
                     !System.Linq.Enumerable.Any(newPassword, Char.IsLetter) ||
                     !System.Linq.Enumerable.Any(newPassword, Char.IsDigit))
                TempData["PasswordError"] = "Mật khẩu mới cần ít nhất 8 ký tự, gồm chữ và số.";
            else if (newPassword != confirmPassword)
                TempData["PasswordError"] = "Mật khẩu xác nhận không khớp.";
            else
            {
                AccountController.DemoUsers[email] = newPassword;
                TempData["PasswordMessage"] = "Đổi mật khẩu thành công.";
            }
            return RedirectToAction("Index");
        }
    }
}
