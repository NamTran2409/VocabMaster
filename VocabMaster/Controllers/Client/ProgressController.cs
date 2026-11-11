using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VocabMaster.ViewModels;


namespace VocabMaster.Controllers
{
    public class ProgressController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserEmail"] == null)
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("Index", "Progress") });

            var topics = LearningData.GetTopics();

            var quizHistory = Session["QuizHistory"] as List<QuizHistoryItem>
                              ?? new List<QuizHistoryItem>();

            // Tính trạng thái từng chủ đề theo điểm quiz.
            foreach (var topic in topics)
            {
                var topicHistory = quizHistory
                    .Where(x => x.TopicId == topic.TopicId)
                    .ToList();

                topic.Progress = topicHistory.Any(x => x.ScorePercentage >= 50)
                    ? 100
                    : topicHistory.Count > 0
                        ? topicHistory.Max(x => x.ScorePercentage)
                        : 0;
            }

            var history = quizHistory
                .OrderByDescending(x => x.CompletedAt)
                .Select(x => new QuizHistoryViewModel
                {
                    Topic = x.QuizTitle,
                    Score = x.ScorePercentage,
                    Correct = x.CorrectCount,
                    Total = x.TotalQuestions,
                    Date = x.CompletedAt.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

            int totalWords = topics.Sum(x => x.WordCount);

            int completedTopics = topics.Count(topic =>
                quizHistory.Any(x =>
                    x.TopicId == topic.TopicId &&
                    x.ScorePercentage >= 50));

            int averageScore = quizHistory.Count == 0
                ? 0
                : (int)Math.Round(
                    quizHistory.Average(x => x.ScorePercentage));

            var days = Enumerable.Range(0, 7)
                .Select(offset => DateTime.Today.AddDays(offset - 6))
                .Select(date => new ProgressDayViewModel
                {
                    Day = date.ToString("dd/MM"),
                    Learned = 0
                }).ToList();

            var model = new MyProgressViewModel
            {
                LearnedWords = 0,
                TotalWords = totalWords,
                CompletedTopics = completedTopics,
                TotalTopics = 12,
                AverageScore = averageScore,
                QuizCount = quizHistory.Count,
                LearnedLast7Days = 0,
                LearnedToday = 0,
                DailyGoal = Convert.ToInt32(Session["DailyGoal"] ?? 10),
                PreviousLearnedWords = 0,
                Days = days,
                Topics = topics,
                QuizHistory = history
            };

            return View(model);
        }
    }
}