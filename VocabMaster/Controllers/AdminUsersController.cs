
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminUsersController : Controller
    {
        // GET: AdminUsers
        public ActionResult Index(string searchString, string role, string status)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var users = db.Users.AsQueryable();

                // Tìm kiếm người dùng
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim();

                    users = users.Where(u =>
                        u.Username.Contains(searchString) ||
                        u.Email.Contains(searchString) ||
                        u.DisplayName.Contains(searchString));
                }

                // Lọc theo vai trò
                if (!string.IsNullOrWhiteSpace(role))
                {
                    users = users.Where(u => u.Role == role);
                }

                // Lọc theo trạng thái
                if (status == "active")
                {
                    users = users.Where(u => u.IsActive);
                }
                else if (status == "locked")
                {
                    users = users.Where(u => !u.IsActive);
                }

                var result = users
                    .OrderByDescending(u => u.CreatedAt)
                    .ToList();

                ViewBag.SearchString = searchString;
                ViewBag.Role = role;
                ViewBag.Status = status;

                ViewBag.TotalUsers = db.Users.Count();
                ViewBag.ActiveUsers = db.Users.Count(u => u.IsActive);
                ViewBag.LockedUsers = db.Users.Count(u => !u.IsActive);

                return View(result);
            }
        }

        // GET: AdminUsers/Details/5
        public ActionResult Details(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var user = db.Users
                    .Include(u => u.QuizAttempts)
                    .Include(u => u.UserVocabularyProgresses)
                    .FirstOrDefault(u => u.UserId == id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                return View(user);
            }
        }

        // POST: AdminUsers/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Không cho khóa tài khoản Admin
                if (user.Role == "Admin")
                {
                    TempData["ErrorMessage"] =
                        "Không thể khóa tài khoản quản trị viên.";
                    return RedirectToAction("Index");
                }

                user.IsActive = !user.IsActive;
                db.SaveChanges();

                TempData["SuccessMessage"] = user.IsActive
                    ? "Đã mở khóa tài khoản thành công!"
                    : "Đã khóa tài khoản thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}
