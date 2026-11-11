
using System;
using System.Collections.Generic;

namespace VocabMaster.ViewModels
{
    public class QuestionItem
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }
        public string Example { get; set; }
    }

    // TH�NG TIN CH? �? QUIZ
    public class QuizTopicItem
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int WordCount { get; set; }
    }

    // VIEWMODEL TRANG QUIZ
    public class QuizViewModel
    {
        public int TopicId { get; set; }
        public string TopicTitle { get; set; }

        public List<QuestionItem> Questions { get; set; }

        public List<QuizHistoryItem> History { get; set; }

        public List<QuizTopicItem> Topics { get; set; }

        public bool IsTakingQuiz { get; set; }
    }

    // L?CH S? L�M B�I
    public class QuizHistoryItem
    {
        public int TopicId { get; set; }
        public string QuizTitle { get; set; }
        public DateTime CompletedAt { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public int ScorePercentage { get; set; }
    }

    public class QuizSubmitModel
    {
        public int TopicId { get; set; }
        public Dictionary<int, int> UserAnswers { get; set; }
    }

    // K?T QU? T?NG C�U H?I
    public class QuestionResultItem
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Phonetic { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswer { get; set; }
        public int UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; }
        public string Example { get; set; }
    }

    // K?T QU? B�I QUIZ
    public class QuizResultViewModel
    {
        public int TopicId { get; set; }
        public string TopicTitle { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int ScorePercentage { get; set; }
        public List<QuestionResultItem> Details { get; set; }
    }
}
