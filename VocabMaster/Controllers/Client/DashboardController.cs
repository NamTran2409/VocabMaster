using System.Linq;
using System.Web.Mvc;
using VocabMaster.ViewModels;

namespace VocabMaster.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            var topics = LearningData.GetTopics();
            var email = Session["UserEmail"] as string;
            var name = Session["FullName"] as string;
            if (string.IsNullOrWhiteSpace(name))
                name = !string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : "Bạn";

            var model = new DashboardViewModel
            {
                DisplayName = name,
                TodayGoal = 20,
                LearnedToday = 8,
                TotalWords = topics.Sum(x => x.WordCount),
                CompletedTopics = topics.Count(x => x.Progress >= 100),
                StreakDays = 7,
                CurrentTopics = topics.Where(x => x.Progress > 0 && x.Progress < 100).Take(3).ToList(),
                TodayWords = LearningData.GetTodayWords(),
                RecentActivities = new System.Collections.Generic.List<string>
                {
                    "Bạn đã ôn tập flashcard gần đây.",
                    "Tiếp tục chủ đề đang học để giữ chuỗi ngày.",
                    "Hoàn thành mục tiêu hôm nay để tăng tiến độ."
                }
            };
            return View("~/Views/ClientViews/Dashboard/Index.cshtml", model);
        }
    }
}
