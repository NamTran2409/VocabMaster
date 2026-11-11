using System.Linq;
using System.Web.Mvc;

namespace VocabMaster.Controllers
{
    public class LearningController : Controller
    {
        public ActionResult VocabularyList(int? topicId, string topicName)
        {
            var topic = LearningData.GetTopics()
                .FirstOrDefault(x => x.TopicId == topicId);

            ViewBag.Title = "Danh sách từ vựng";
            ViewBag.Active = "VocabularyList";

            ViewBag.TopicId = topicId ??
                (topic != null ? topic.TopicId : 4);

            ViewBag.TopicName = !string.IsNullOrWhiteSpace(topicName)
                ? topicName
                : (topic != null ? topic.TopicName : "Du lịch");

            return View("~/Views/ClientViews/Learning/VocabularyList.cshtml");
        }

        public ActionResult Flashcards(int? topicId, string topicName)
        {
            var topic = LearningData.GetTopics()
                .FirstOrDefault(x => x.TopicId == topicId);

            ViewBag.Title = "Flashcards";
            ViewBag.Active = "Flashcards";

            ViewBag.TopicId = topicId ??
                (topic != null ? topic.TopicId : 4);

            ViewBag.TopicName = !string.IsNullOrWhiteSpace(topicName)
                ? topicName
                : (topic != null ? topic.TopicName : "Du lịch");

            return View("~/Views/ClientViews/Learning/Flashcards.cshtml");
        }
    }
}
