
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
                Status = "Active"
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
                    topic.CreatedAt = DateTime.Now;

                    db.Topics.Add(topic);
                    db.SaveChanges();
                }

                TempData["SuccessMessage"] =
                    "Thêm chủ đề mới thành công!";

                return RedirectToAction("Index");
            }

            return View(topic);
        }
    }
}
