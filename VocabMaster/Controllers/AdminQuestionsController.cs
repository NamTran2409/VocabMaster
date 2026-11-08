
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminQuestionsController : Controller
    {
        // DANH SACH BAI KIEM TRA
        public ActionResult Index(string searchString, int? topicId)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var quizzes = db.Quizzes
                    .Include(q => q.Topic)
                    .Include(q => q.Questions)
                    .AsQueryable();

                // Tim kiem theo ten bai kiem tra
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim();

                    quizzes = quizzes.Where(q =>
                        q.QuizTitle.Contains(searchString));
                }

                // Loc theo chu de
                if (topicId.HasValue)
                {
                    quizzes = quizzes.Where(q =>
                        q.TopicId == topicId.Value);
                }

                var quizList = quizzes
                    .OrderByDescending(q => q.CreatedAt)
                    .ToList();

                // Du lieu chu de cho bo loc
                ViewBag.Topics = db.Topics
                    .OrderBy(t => t.TopicName)
                    .ToList();

                ViewBag.TopicCount = db.Topics.Count();
                ViewBag.SearchString = searchString;
                ViewBag.SelectedTopicId = topicId;

                return View(quizList);
            }
        }
    }
}
