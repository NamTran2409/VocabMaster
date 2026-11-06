using System.Linq;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminTopicsController : Controller
    {
        public ActionResult Index(string searchString)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var topics = db.Topics.AsQueryable();

                // Tìm kiếm theo tên chủ đề hoặc mô tả
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    topics = topics.Where(t =>
                        t.TopicName.Contains(searchString) ||
                        t.Description.Contains(searchString));
                }

                // Sắp xếp theo thứ tự hiển thị
                var topicList = topics
                    .OrderBy(t => t.SortOrder)
                    .ThenBy(t => t.TopicName)
                    .ToList();

                ViewBag.SearchString = searchString;

                return View(topicList);
            }
        }
    }
}