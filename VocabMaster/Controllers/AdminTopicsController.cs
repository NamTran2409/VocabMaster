
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminTopicsController : Controller
    {
        // DANH SÁCH CHỦ ĐỀ
        public ActionResult Index(string searchString)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var topics = db.Topics
                    .Include(t => t.Vocabularies)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    topics = topics.Where(t =>
                        t.TopicName.Contains(searchString) ||
                        t.Description.Contains(searchString));
                }

                var topicList = topics
                    .OrderBy(t => t.SortOrder)
                    .ThenBy(t => t.TopicName)
                    .ToList();

                ViewBag.SearchString = searchString;

                return View(topicList);
            }
        }

        // HIỂN THỊ FORM THÊM CHỦ ĐỀ
        [HttpGet]
        public ActionResult Create()
        {
            var topic = new Topic
            {
                SortOrder = 0,
                Status = "Published"
            };

            return View(topic);
        }

        // LƯU CHỦ ĐỀ MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Topic topic)
        {
            if (string.IsNullOrWhiteSpace(topic.TopicName))
            {
                ModelState.AddModelError(
                    "TopicName",
                    "Vui lòng nhập tên chủ đề."
                );
            }

            if (ModelState.IsValid)
            {
                using (var db = new VocabMasterDBEntities())
                {
                    topic.TopicName = topic.TopicName.Trim();

                    bool isDuplicate = db.Topics.Any(t =>
                        t.TopicName.ToLower() == topic.TopicName.ToLower()
                    );

                    if (isDuplicate)
                    {
                        ModelState.AddModelError(
                            "TopicName",
                            "Tên chủ đề này đã tồn tại. Vui lòng nhập tên khác."
                        );

                        return View(topic);
                    }

                    topic.CreatedAt = DateTime.Now;

                    db.Topics.Add(topic);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException)
                    {
                        ModelState.AddModelError(
                            "TopicName",
                            "Tên chủ đề đã tồn tại hoặc dữ liệu không hợp lệ."
                        );

                        return View(topic);
                    }
                }

                TempData["SuccessMessage"] =
                    "Thêm chủ đề mới thành công!";

                return RedirectToAction("Index");
            }

            return View(topic);
        }

        // HIỂN THỊ FORM SỬA CHỦ ĐỀ
        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var topic = db.Topics.Find(id);

                if (topic == null)
                {
                    return HttpNotFound();
                }

                return View(topic);
            }
        }

        // CẬP NHẬT CHỦ ĐỀ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Topic topic)
        {
            if (string.IsNullOrWhiteSpace(topic.TopicName))
            {
                ModelState.AddModelError(
                    "TopicName",
                    "Vui lòng nhập tên chủ đề."
                );
            }

            if (ModelState.IsValid)
            {
                using (var db = new VocabMasterDBEntities())
                {
                    var existingTopic = db.Topics.Find(topic.TopicId);

                    if (existingTopic == null)
                    {
                        return HttpNotFound();
                    }

                    topic.TopicName = topic.TopicName.Trim();

                    bool isDuplicate = db.Topics.Any(t =>
                        t.TopicId != topic.TopicId &&
                        t.TopicName.ToLower() == topic.TopicName.ToLower()
                    );

                    if (isDuplicate)
                    {
                        ModelState.AddModelError(
                            "TopicName",
                            "Tên chủ đề này đã được sử dụng."
                        );

                        return View(topic);
                    }

                    // Cập nhật thông tin chủ đề
                    existingTopic.TopicName = topic.TopicName;
                    existingTopic.Description = topic.Description;
                    existingTopic.IconUrl = topic.IconUrl;
                    existingTopic.SortOrder = topic.SortOrder;
                    existingTopic.Status = topic.Status;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException)
                    {
                        ModelState.AddModelError(
                            "TopicName",
                            "Không thể cập nhật. Vui lòng kiểm tra lại dữ liệu."
                        );

                        return View(topic);
                    }
                }

                TempData["SuccessMessage"] =
                    "Cập nhật chủ đề thành công!";

                return RedirectToAction("Index");
            }

            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var topic = db.Topics.Find(id);

                if (topic == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra chủ đề có từ vựng hoặc bài kiểm tra không
                bool hasVocabularies = topic.Vocabularies.Any();
                bool hasQuizzes = topic.Quizzes.Any();

                if (hasVocabularies || hasQuizzes)
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa chủ đề vì vẫn còn từ vựng hoặc bài kiểm tra liên quan.";

                    return RedirectToAction("Index");
                }

                db.Topics.Remove(topic);
                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Xóa chủ đề thành công!";

            return RedirectToAction("Index");
        }
    }
}
