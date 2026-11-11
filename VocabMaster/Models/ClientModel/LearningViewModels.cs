using System;
using System.Collections.Generic;

namespace VocabMaster.ViewModels
{
    public class TopicCardViewModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int WordCount { get; set; }
        public int Progress { get; set; }
        public string Status
        {
            get { return Progress >= 100 ? "completed" : (Progress > 0 ? "learning" : "new"); }
        }
        public string StatusText
        {
            get { return Progress >= 100 ? "Hoàn thành" : (Progress > 0 ? "Đang học" : "Chưa học"); }
        }
    }

    public class DashboardViewModel
    {
        public string DisplayName { get; set; }
        public int TodayGoal { get; set; }
        public int LearnedToday { get; set; }
        public int TotalWords { get; set; }
        public int CompletedTopics { get; set; }
        public int StreakDays { get; set; }
        public List<TopicCardViewModel> CurrentTopics { get; set; }
        public List<string> RecentActivities { get; set; }
        public List<string> TodayWords { get; set; }
    }
}


namespace VocabMaster.ViewModels
{
    public class ProgressDayViewModel
    {
        public string Day { get; set; }
        public int Learned { get; set; }
    }

    public class QuizHistoryViewModel
    {
        public string Topic { get; set; }
        public int Score { get; set; }
        public int Correct { get; set; }
        public int Total { get; set; }
        public string Date { get; set; }
    }

    public class MyProgressViewModel
    {
        public int LearnedWords { get; set; }
        public int TotalWords { get; set; }
        public int CompletedTopics { get; set; }
        public int TotalTopics { get; set; }
        public int AverageScore { get; set; }
        public int QuizCount { get; set; }
        public int LearnedLast7Days { get; set; }
        public int LearnedToday { get; set; }
        public int DailyGoal { get; set; }
        public int PreviousLearnedWords { get; set; }
        public System.Collections.Generic.List<ProgressDayViewModel> Days { get; set; }
        public System.Collections.Generic.List<TopicCardViewModel> Topics { get; set; }
        public System.Collections.Generic.List<QuizHistoryViewModel> QuizHistory { get; set; }
    }

    public class ProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
        public int DailyGoal { get; set; }
        public string ReminderTime { get; set; }
        public bool EmailReminder { get; set; }
        public bool PronunciationSound { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
}
