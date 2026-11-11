
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VocabMaster.ViewModels;

namespace VocabMaster.Controllers
{
    public class QuizController : Controller
    {
        // DANH SÁCH CÂU HỎI MẪU
        private List<QuestionItem> GetSampleQuestions()
        {
            return new List<QuestionItem>
            {
                new QuestionItem
                {
                    Id = 1,
                    Word = "Perseverance",
                    Phonetic = "/ˌpɜː.sɪˈvɪə.rəns/",
                    QuestionText = "Nghĩa của từ 'Perseverance' là gì?",
                    Options = new List<string>
                    {
                        "Kiên trì, bền bỉ",
                        "Sự may mắn",
                        "Thất bại",
                        "Sự lười biếng"
                    },
                    CorrectIndex = 0,
                    Explanation = "'Perseverance' có nghĩa là sự kiên trì, nỗ lực không ngừng nghỉ.",
                    Example = "Success requires hard work and perseverance."
                },

                new QuestionItem
                {
                    Id = 2,
                    Word = "Meticulous",
                    Phonetic = "/məˈtɪk.jə.ləs/",
                    QuestionText = "Điền từ đúng vào câu: 'She was extremely _____ in her research.'",
                    Options = new List<string>
                    {
                        "Careless",
                        "Meticulous",
                        "Quick",
                        "Lazy"
                    },
                    CorrectIndex = 1,
                    Explanation = "'Meticulous' nghĩa là tỉ mỉ, cẩn thận từng chi tiết nhỏ.",
                    Example = "He is meticulous about his appearance."
                },

                new QuestionItem
                {
                    Id = 3,
                    Word = "Ambiguous",
                    Phonetic = "/æmˈbɪɡ.ju.əs/",
                    QuestionText = "Từ nào ĐỒNG NGHĨA với 'Ambiguous' (mơ hồ, khó hiểu)?",
                    Options = new List<string>
                    {
                        "Clear",
                        "Vague",
                        "Explicit",
                        "Simple"
                    },
                    CorrectIndex = 1,
                    Explanation = "'Vague' và 'Ambiguous' đều chỉ trạng thái mơ hồ, không rõ ràng.",
                    Example = "His answer was ambiguous."
                },

                new QuestionItem
                {
                    Id = 4,
                    Word = "Resilience",
                    Phonetic = "/rɪˈzɪl.jəns/",
                    QuestionText = "Nghĩa của từ 'Resilience' là gì?",
                    Options = new List<string>
                    {
                        "Khả năng phục hồi / kiên cường",
                        "Sự yếu đuối",
                        "Tính hung hăng",
                        "Sự nghi ngờ"
                    },
                    CorrectIndex = 0,
                    Explanation = "'Resilience' là khả năng nhanh chóng hồi phục sau khó khăn.",
                    Example = "Courage and resilience helped them survive."
                },

                new QuestionItem
                {
                    Id = 5,
                    Word = "Eloquent",
                    Phonetic = "/ˈel.ə.kwənt/",
                    QuestionText = "Từ 'Eloquent' mô tả người như thế nào?",
                    Options = new List<string>
                    {
                        "Nói lắp",
                        "Hùng hồn, trôi chảy",
                        "Im lặng",
                        "Ngốc nghếch"
                    },
                    CorrectIndex = 1,
                    Explanation = "'Eloquent' miêu tả khả năng diễn đạt lưu loát, giàu sức thuyết phục.",
                    Example = "An eloquent speech inspired the audience."
                },

                new QuestionItem
                {
                    Id = 6,
                    Word = "Pragmatic",
                    Phonetic = "/præɡˈmæt.ɪk/",
                    QuestionText = "Từ nào TRÁI NGHĨA với 'Pragmatic' (thực tế)?",
                    Options = new List<string>
                    {
                        "Practical",
                        "Realistic",
                        "Idealistic",
                        "Sensible"
                    },
                    CorrectIndex = 2,
                    Explanation = "'Pragmatic' là thực tế, còn 'Idealistic' là duy tâm, lý tưởng hóa.",
                    Example = "We need a pragmatic approach to this problem."
                },

                new QuestionItem
                {
                    Id = 7,
                    Word = "Ephemeral",
                    Phonetic = "/ɪˈfem.ər.əl/",
                    QuestionText = "Nghĩa của từ 'Ephemeral' là gì?",
                    Options = new List<string>
                    {
                        "Vĩnh cửu",
                        "Phù du, chóng khánh",
                        "Dài lâu",
                        "Cố định"
                    },
                    CorrectIndex = 1,
                    Explanation = "'Ephemeral' chỉ những thứ kéo dài trong thời gian rất ngắn.",
                    Example = "Fame in the world of pop music is ephemeral."
                },

                new QuestionItem
                {
                    Id = 8,
                    Word = "Ubiquitous",
                    Phonetic = "/juːˈbɪk.wɪ.təs/",
                    QuestionText = "Từ 'Ubiquitous' có nghĩa là gì?",
                    Options = new List<string>
                    {
                        "Có mặt ở khắp nơi",
                        "Hiếm có",
                        "Đắt đỏ",
                        "Ẩn giấu"
                    },
                    CorrectIndex = 0,
                    Explanation = "'Ubiquitous' nghĩa là xuất hiện ở khắp mọi nơi cùng lúc.",
                    Example = "Smartphones have become ubiquitous in daily life."
                },

                new QuestionItem
                {
                    Id = 9,
                    Word = "Candor",
                    Phonetic = "/ˈkæn.dər/",
                    QuestionText = "Từ 'Candor' có nghĩa là gì?",
                    Options = new List<string>
                    {
                        "Sự dối trá",
                        "Sự bộc trực, chân thành",
                        "Sự tức giận",
                        "Tính kiêu ngạo"
                    },
                    CorrectIndex = 1,
                    Explanation = "'Candor' là tính thật thà, thẳng thắn, không giấu giếm.",
                    Example = "I appreciate the candor of your comments."
                },

                new QuestionItem
                {
                    Id = 10,
                    Word = "Lucid",
                    Phonetic = "/ˈluː.sɪd/",
                    QuestionText = "Từ 'Lucid' trong 'Lucid explanation' nghĩa là gì?",
                    Options = new List<string>
                    {
                        "Rõ ràng, dễ hiểu",
                        "Phức tạp",
                        "Tối nghĩa",
                        "Kì lạ"
                    },
                    CorrectIndex = 0,
                    Explanation = "'Lucid' biểu thị sự minh mẫn, rõ ràng, dễ tiếp thu.",
                    Example = "She gave a lucid description of the situation."
                }
            };
        }

        // TRANG DANH SÁCH QUIZ
        public ActionResult Index(bool start = false, int topicId = 1)
        {
            var history = Session["QuizHistory"] as List<QuizHistoryItem>
                          ?? new List<QuizHistoryItem>();

            var topics = LearningData.GetTopics();

            var selectedTopic = topics.FirstOrDefault(
                x => x.TopicId == topicId
            );

            var model = new QuizViewModel
            {
                TopicId = topicId,

                TopicTitle = selectedTopic != null
                    ? selectedTopic.TopicName
                    : "Chủ đề từ vựng",

                Topics = topics.Select(x => new QuizTopicItem
                {
                    TopicId = x.TopicId,
                    TopicName = x.TopicName,
                    WordCount = x.WordCount
                }).ToList(),

                Questions = start
                    ? GetSampleQuestions()
                    : new List<QuestionItem>(),

                History = history
                    .OrderByDescending(x => x.CompletedAt)
                    .ToList(),

                IsTakingQuiz = start
            };

            return View("~/Views/ClientViews/Quiz/Index.cshtml", model);
        }

        // CHẤM ĐIỂM VÀ HIỂN THỊ KẾT QUẢ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Result(FormCollection form)
        {
            var questions = GetSampleQuestions();

            var details = new List<QuestionResultItem>();
            int correctCount = 0;

            foreach (var q in questions)
            {
                string inputKey = "q_" + q.Id;
                int userAns = -1;

                if (!string.IsNullOrEmpty(form[inputKey]))
                {
                    int.TryParse(form[inputKey], out userAns);
                }

                bool isCorrect = (userAns == q.CorrectIndex);

                if (isCorrect)
                {
                    correctCount++;
                }

                details.Add(new QuestionResultItem
                {
                    Id = q.Id,
                    Word = q.Word,
                    Phonetic = q.Phonetic,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    UserAnswer = userAns,
                    CorrectAnswer = q.CorrectIndex,
                    IsCorrect = isCorrect,
                    Explanation = q.Explanation,
                    Example = q.Example
                });
            }

            int topicId = 1;
            int.TryParse(form["TopicId"], out topicId);

            if (topicId <= 0)
            {
                topicId = 1;
            }

            var selectedTopic = LearningData.GetTopics()
                .FirstOrDefault(x => x.TopicId == topicId);

            string topicTitle = selectedTopic != null
                ? selectedTopic.TopicName
                : "Chủ đề từ vựng";

            var resultModel = new QuizResultViewModel
            {
                TopicId = topicId,
                TopicTitle = topicTitle,
                TotalQuestions = questions.Count,
                CorrectCount = correctCount,
                ScorePercentage = (int)(
                    (double)correctCount / questions.Count * 100
                ),
                Details = details
            };

            var history = Session["QuizHistory"] as List<QuizHistoryItem>
                          ?? new List<QuizHistoryItem>();

            history.Add(new QuizHistoryItem
            {
                TopicId = topicId,
                QuizTitle = resultModel.TopicTitle,
                CompletedAt = DateTime.Now,
                CorrectCount = correctCount,
                TotalQuestions = questions.Count,
                ScorePercentage = resultModel.ScorePercentage
            });

            Session["QuizHistory"] = history;

            return View("~/Views/ClientViews/Quiz/Result.cshtml", resultModel);
        }
    }
}
