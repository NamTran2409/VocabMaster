using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminTopicsController : Controller
    {
        public ActionResult Index(string searchString)
        {
            using (var db = new VocabMasterDBEntities())
            {
                // Tải cả danh sách từ vựng của từng chủ đề
                // trước khi đóng kết nối Database.
                var topics = db.Topics
                    .Include(t => t.Vocabularies)
                    .AsQueryable();

                // Tìm kiếm theo tên hoặc mô tả
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    topics = topics.Where(t =>
                        t.TopicName.Contains(searchString) ||
                        t.Description.Contains(searchString));
                }

                // Sắp xếp và lấy dữ liệu
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
