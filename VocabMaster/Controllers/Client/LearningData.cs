using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using VocabMaster.ViewModels;

namespace VocabMaster.Controllers
{
    internal static class LearningData
    {
        public static List<TopicCardViewModel> GetTopics()
        {
            var result = new List<TopicCardViewModel>();
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["VocabMasterConnection"];
                if (cs != null)
                {
                    using (var cn = new SqlConnection(cs.ConnectionString))
                    using (var cmd = new SqlCommand(@"
SELECT t.TopicId, t.TopicName, COUNT(v.TopicId) AS WordCount
FROM dbo.Topics t
LEFT JOIN dbo.Vocabulary v ON v.TopicId = t.TopicId
GROUP BY t.TopicId, t.TopicName, t.SortOrder
ORDER BY t.SortOrder, t.TopicName", cn))
                    {
                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                        {
                            int index = 0;
                            while (rd.Read())
                            {
                                result.Add(new TopicCardViewModel
                                {
                                    TopicId = Convert.ToInt32(rd["TopicId"]),
                                    TopicName = Convert.ToString(rd["TopicName"]),
                                    WordCount = Convert.ToInt32(rd["WordCount"]),
                                    Progress = DemoProgress(index++)
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            if (result.Count == 0)
            {
                string[] names = { "Gia đình", "Trường học", "Công việc", "Du lịch", "Ẩm thực", "Công nghệ", "Sức khỏe", "Thể thao", "Mua sắm", "Giao tiếp", "Thiên nhiên", "Cảm xúc" };
                int[] words = { 42, 55, 48, 60, 38, 52, 44, 50, 36, 58, 40, 46 };
                for (int i = 0; i < names.Length; i++)
                    result.Add(new TopicCardViewModel { TopicId = i + 1, TopicName = names[i], WordCount = words[i], Progress = DemoProgress(i) });
            }
            return result;
        }

        public static List<string> GetTodayWords()
        {
            var words = new List<string>();
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["VocabMasterConnection"];
                if (cs != null)
                {
                    using (var cn = new SqlConnection(cs.ConnectionString))
                    using (var cmd = new SqlCommand("SELECT TOP 5 EnglishWord FROM dbo.Vocabulary ORDER BY NEWID()", cn))
                    {
                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read()) words.Add(Convert.ToString(rd[0]));
                    }
                }
            }
            catch { }
            if (words.Count == 0) words.AddRange(new[] { "achievement", "journey", "improve", "confident", "practice" });
            return words;
        }

        private static int DemoProgress(int index)
        {
            int[] values = { 65, 100, 35, 0, 80, 20, 100, 45, 0, 70, 10, 55 };
            return values[index % values.Length];
        }
    }
}
