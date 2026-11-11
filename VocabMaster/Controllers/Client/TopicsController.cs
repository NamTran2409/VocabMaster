
using System.Linq;
using System.Web.Mvc;

namespace VocabMaster.Controllers
{
    public class TopicsController : Controller
    {
        public ActionResult Index(string q = "", string status = "all")
        {
            var topics = LearningData.GetTopics().AsEnumerable();

            q = (q ?? "").Trim();
            status = (status ?? "all").ToLowerInvariant();

            if (q.Length > 0)
                topics = topics.Where(x =>
                    x.TopicName.IndexOf(q, System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (status == "learning")
                topics = topics.Where(x => x.Progress > 0 && x.Progress < 100);
            else if (status == "completed")
                topics = topics.Where(x => x.Progress >= 100);

            ViewBag.Query = q;
            ViewBag.Status = status;

            return View("~/Views/ClientViews/Topics/Index.cshtml", topics.ToList());
        }
    }
}
