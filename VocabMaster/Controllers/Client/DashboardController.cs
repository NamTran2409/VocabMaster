using System.Linq;
using System.Web.Mvc;
using VocabMaster.ViewModels;
using System.Collections.Generic;

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
            {
                name = !string.IsNullOrWhiteSpace(email)
                    ? email.Split('@')[0]
                    : "Bạn";
            }

            var model = new DashboardViewModel
            {
                // TÊN HỌC VIÊN
                DisplayName = name,

                // MỤC TIÊU HỌC HÔM NAY
                TodayGoal = 10,

                // TÀI KHOẢN MỚI CHƯA HỌC
                LearnedToday = 0,

                // TỔNG SỐ TỪ VỰNG CÓ TRONG HỆ THỐNG
                TotalWords = topics.Sum(x => x.WordCount),

                // CHƯA HOÀN THÀNH CHỦ ĐỀ NÀO
                CompletedTopics = 0,

                // CHƯA CÓ CHUỖI NGÀY HỌC
                StreakDays = 0,

                // CHƯA CÓ CHỦ ĐỀ ĐANG HỌC
                CurrentTopics = new List<TopicCardViewModel>(),

                // TỪ VỰNG GỢI Ý HÔM NAY
                TodayWords = LearningData.GetTodayWords(),

                // CHƯA CÓ HOẠT ĐỘNG GẦN ĐÂY
                RecentActivities = new List<string>()
            };

            return View(
                "~/Views/ClientViews/Dashboard/Index.cshtml",
                model
            );
        }
    }
}