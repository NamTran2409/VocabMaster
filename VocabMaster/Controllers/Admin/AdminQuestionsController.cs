
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminQuestionsController : Controller
    {
        // ==========================================
        // 1. DANH SACH BAI KIEM TRA
        // ==========================================
        public ActionResult Index(string searchString, int? topicId)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var quizzes = db.Quizzes
                    .Include(q => q.Topic)
                    .Include(q => q.Questions)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim();
                    quizzes = quizzes.Where(q =>
                        q.QuizTitle.Contains(searchString));
                }

                if (topicId.HasValue)
                {
                    quizzes = quizzes.Where(q =>
                        q.TopicId == topicId.Value);
                }

                var quizList = quizzes
                    .OrderByDescending(q => q.CreatedAt)
                    .ToList();

                ViewBag.Topics = db.Topics
                    .OrderBy(t => t.TopicName)
                    .ToList();

                ViewBag.TopicCount = db.Topics.Count();
                ViewBag.SearchString = searchString;
                ViewBag.SelectedTopicId = topicId;

                return View(quizList);
            }
        }

        // ==========================================
        // 2. XEM CAU HOI CUA QUIZ
        // ==========================================
        public ActionResult Questions(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var quiz = db.Quizzes
                    .Include(q => q.Topic)
                    .FirstOrDefault(q => q.QuizId == id);

                if (quiz == null)
                {
                    return HttpNotFound();
                }

                var questions = db.Questions
                    .Include(q => q.QuestionOptions)
                    .Where(q => q.QuizId == id)
                    .OrderBy(q => q.SortOrder)
                    .ToList();

                ViewBag.Quiz = quiz;
                return View(questions);
            }
        }

        // ==========================================
        // 3. HIEN THI FORM THEM CAU HOI
        // ==========================================
        [HttpGet]
        public ActionResult CreateQuestion(int quizId)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var quiz = db.Quizzes
                    .Include(q => q.Topic)
                    .FirstOrDefault(q => q.QuizId == quizId);

                if (quiz == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Quiz = quiz;

                var model = new QuestionCreateViewModel
                {
                    QuizId = quizId,
                    CorrectOption = "A"
                };

                return View(model);
            }
        }

        // ==========================================
        // 4. LUU CAU HOI MOI VA 4 DAP AN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateQuestion(QuestionCreateViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(
                    400, "Dữ liệu câu hỏi không hợp lệ.");
            }

            if (model.QuizId <= 0)
            {
                return new HttpStatusCodeResult(
                    400, "Mã bài kiểm tra không hợp lệ.");
            }

            ValidateQuestionModel(model);

            using (var db = new VocabMasterDBEntities())
            {
                var quiz = db.Quizzes
                    .Include(q => q.Topic)
                    .FirstOrDefault(q => q.QuizId == model.QuizId);

                if (quiz == null)
                {
                    return HttpNotFound();
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Quiz = quiz;
                    return View(model);
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        int nextSortOrder = (db.Questions
                            .Where(q => q.QuizId == model.QuizId)
                            .Select(q => (int?)q.SortOrder)
                            .Max() ?? 0) + 1;

                        var question = new Question
                        {
                            QuizId = model.QuizId,
                            QuestionText = model.QuestionText.Trim(),
                            Explanation = string.IsNullOrWhiteSpace(model.Explanation)
                                ? null
                                : model.Explanation.Trim(),
                            SortOrder = nextSortOrder
                        };

                        db.Questions.Add(question);
                        db.SaveChanges();

                        AddQuestionOption(
                            db, question.QuestionId, "A",
                            model.OptionA, model.CorrectOption);

                        AddQuestionOption(
                            db, question.QuestionId, "B",
                            model.OptionB, model.CorrectOption);

                        AddQuestionOption(
                            db, question.QuestionId, "C",
                            model.OptionC, model.CorrectOption);

                        AddQuestionOption(
                            db, question.QuestionId, "D",
                            model.OptionD, model.CorrectOption);

                        db.SaveChanges();

                        quiz.QuestionCount = db.Questions
                            .Count(q => q.QuizId == model.QuizId);

                        db.SaveChanges();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            TempData["SuccessMessage"] =
                "Thêm câu hỏi và 4 đáp án thành công!";

            return RedirectToAction(
                "Questions", new { id = model.QuizId });
        }

        // ==========================================
        // 5. HIEN THI FORM SUA CAU HOI
        // ==========================================
        [HttpGet]
        public ActionResult EditQuestion(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var question = db.Questions
                    .Include(q => q.QuestionOptions)
                    .FirstOrDefault(q => q.QuestionId == id);

                if (question == null)
                {
                    return HttpNotFound();
                }

                var quiz = db.Quizzes
                    .Include(q => q.Topic)
                    .FirstOrDefault(q => q.QuizId == question.QuizId);

                if (quiz == null)
                {
                    return HttpNotFound();
                }

                var options = question.QuestionOptions.ToList();

                var model = new QuestionCreateViewModel
                {
                    QuestionId = question.QuestionId,
                    QuizId = question.QuizId,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,

                    OptionA = GetOptionText(options, "A"),
                    OptionB = GetOptionText(options, "B"),
                    OptionC = GetOptionText(options, "C"),
                    OptionD = GetOptionText(options, "D"),

                    CorrectOption = GetCorrectOption(options)
                };

                ViewBag.Quiz = quiz;

                return View(model);
            }
        }

        // ==========================================
        // 6. LUU NOI DUNG SAU KHI SUA CAU HOI
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditQuestion(QuestionCreateViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(
                    400, "Dữ liệu câu hỏi không hợp lệ.");
            }

            if (model.QuestionId <= 0 || model.QuizId <= 0)
            {
                return new HttpStatusCodeResult(
                    400, "Mã câu hỏi hoặc bài kiểm tra không hợp lệ.");
            }

            ValidateQuestionModel(model);

            using (var db = new VocabMasterDBEntities())
            {
                var question = db.Questions
                    .FirstOrDefault(q =>
                        q.QuestionId == model.QuestionId &&
                        q.QuizId == model.QuizId);

                if (question == null)
                {
                    return HttpNotFound();
                }

                var quiz = db.Quizzes
                    .Include(q => q.Topic)
                    .FirstOrDefault(q => q.QuizId == model.QuizId);

                if (quiz == null)
                {
                    return HttpNotFound();
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Quiz = quiz;
                    return View(model);
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Cap nhat noi dung cau hoi
                        question.QuestionText = model.QuestionText.Trim();

                        question.Explanation =
                            string.IsNullOrWhiteSpace(model.Explanation)
                            ? null
                            : model.Explanation.Trim();

                        // Lay cac dap an hien co
                        var options = db.QuestionOptions
                            .Where(o => o.QuestionId == model.QuestionId)
                            .ToList();

                        // Cap nhat hoac bo sung 4 dap an
                        SaveQuestionOption(
                            db, options, model.QuestionId,
                            "A", model.OptionA, model.CorrectOption);

                        SaveQuestionOption(
                            db, options, model.QuestionId,
                            "B", model.OptionB, model.CorrectOption);

                        SaveQuestionOption(
                            db, options, model.QuestionId,
                            "C", model.OptionC, model.CorrectOption);

                        SaveQuestionOption(
                            db, options, model.QuestionId,
                            "D", model.OptionD, model.CorrectOption);

                        db.SaveChanges();

                        // Dong bo lai so luong cau hoi
                        quiz.QuestionCount = db.Questions
                            .Count(q => q.QuizId == model.QuizId);

                        db.SaveChanges();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            TempData["SuccessMessage"] =
                "Cập nhật câu hỏi thành công!";

            return RedirectToAction(
                "Questions", new { id = model.QuizId });
        }


        // ==========================================
        // 7. XOA CAU HOI
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQuestion(int questionId, int quizId)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var question = db.Questions.FirstOrDefault(q =>
                    q.QuestionId == questionId &&
                    q.QuizId == quizId);

                if (question == null)
                {
                    return HttpNotFound();
                }

                // Kiem tra lich su tra loi cua nguoi hoc
                bool hasAnswerHistory = db.QuizAttemptAnswers
                    .Any(a => a.QuestionId == questionId);

                if (hasAnswerHistory)
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa câu hỏi vì đã có người học trả lời câu hỏi này.";

                    return RedirectToAction(
                        "Questions", new { id = quizId });
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Xoa cac dap an A, B, C, D
                        var options = db.QuestionOptions
                            .Where(o => o.QuestionId == questionId)
                            .ToList();

                        db.QuestionOptions.RemoveRange(options);

                        // Xoa cau hoi
                        db.Questions.Remove(question);

                        db.SaveChanges();

                        // Dong bo lai so luong cau hoi
                        var quiz = db.Quizzes
                            .FirstOrDefault(q => q.QuizId == quizId);

                        if (quiz != null)
                        {
                            quiz.QuestionCount = db.Questions
                                .Count(q => q.QuizId == quizId);

                            db.SaveChanges();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            TempData["SuccessMessage"] =
                "Xóa câu hỏi thành công!";

            return RedirectToAction(
                "Questions", new { id = quizId });
        }


        // ==========================================
        // 7. HIEN THI FORM THEM BAI KIEM TRA
        // ==========================================
        [HttpGet]
        public ActionResult Create()
        {
            LoadTopics();
            return View();
        }

        // ==========================================
        // 8. LUU BAI KIEM TRA
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            Quizze quiz, int? timeLimitMinutes)
        {
            if (quiz == null)
            {
                return new HttpStatusCodeResult(
                    400, "Dữ liệu bài kiểm tra không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(quiz.QuizTitle))
            {
                ModelState.AddModelError(
                    "QuizTitle",
                    "Vui lòng nhập tên bài kiểm tra.");
            }

            if (quiz.TopicId.HasValue)
            {
                using (var db = new VocabMasterDBEntities())
                {
                    bool topicExists = db.Topics.Any(t =>
                        t.TopicId == quiz.TopicId.Value);

                    if (!topicExists)
                    {
                        ModelState.AddModelError(
                            "TopicId",
                            "Chủ đề được chọn không tồn tại.");
                    }
                }
            }

            if (timeLimitMinutes.HasValue &&
                (timeLimitMinutes.Value < 1 ||
                 timeLimitMinutes.Value > 35791394))
            {
                ModelState.AddModelError(
                    "TimeLimitSeconds",
                    "Thời gian phải từ 1 phút trở lên.");
            }

            if (quiz.Status != "Published" &&
                quiz.Status != "Draft")
            {
                quiz.Status = "Draft";
            }

            if (!ModelState.IsValid)
            {
                LoadTopics();
                return View(quiz);
            }

            using (var db = new VocabMasterDBEntities())
            {
                quiz.QuizTitle = quiz.QuizTitle.Trim();
                quiz.QuestionCount = 0;
                quiz.CreatedAt = DateTime.Now;

                quiz.TimeLimitSeconds = timeLimitMinutes.HasValue
                    ? (int?)(timeLimitMinutes.Value * 60)
                    : null;

                db.Quizzes.Add(quiz);
                db.SaveChanges();
            }

            TempData["SuccessMessage"] =
                "Thêm bài kiểm tra thành công!";

            return RedirectToAction("Index");
        }

        // ==========================================
        // 9. HAM KIEM TRA DU LIEU CAU HOI
        // ==========================================
        private void ValidateQuestionModel(
            QuestionCreateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.QuestionText))
            {
                ModelState.AddModelError(
                    "QuestionText",
                    "Vui lòng nhập nội dung câu hỏi.");
            }

            if (string.IsNullOrWhiteSpace(model.OptionA))
            {
                ModelState.AddModelError(
                    "OptionA", "Vui lòng nhập đáp án A.");
            }

            if (string.IsNullOrWhiteSpace(model.OptionB))
            {
                ModelState.AddModelError(
                    "OptionB", "Vui lòng nhập đáp án B.");
            }

            if (string.IsNullOrWhiteSpace(model.OptionC))
            {
                ModelState.AddModelError(
                    "OptionC", "Vui lòng nhập đáp án C.");
            }

            if (string.IsNullOrWhiteSpace(model.OptionD))
            {
                ModelState.AddModelError(
                    "OptionD", "Vui lòng nhập đáp án D.");
            }

            if (model.CorrectOption != "A" &&
                model.CorrectOption != "B" &&
                model.CorrectOption != "C" &&
                model.CorrectOption != "D")
            {
                ModelState.AddModelError(
                    "CorrectOption",
                    "Vui lòng chọn đáp án đúng.");
            }
        }

        // ==========================================
        // 10. HAM THEM DAP AN
        // ==========================================
        private void AddQuestionOption(
            VocabMasterDBEntities db,
            int questionId,
            string label,
            string text,
            string correctOption)
        {
            db.QuestionOptions.Add(new QuestionOption
            {
                QuestionId = questionId,
                OptionLabel = label,
                OptionText = text.Trim(),
                IsCorrect = correctOption == label
            });
        }

        // ==========================================
        // 11. HAM LAY NOI DUNG DAP AN
        // ==========================================
        private string GetOptionText(
            System.Collections.Generic.List<QuestionOption> options,
            string label)
        {
            var option = options.FirstOrDefault(
                o => o.OptionLabel == label);

            return option == null ? "" : option.OptionText;
        }

        // ==========================================
        // 12. HAM LAY DAP AN DUNG
        // ==========================================
        private string GetCorrectOption(
            System.Collections.Generic.List<QuestionOption> options)
        {
            var correct = options.FirstOrDefault(o => o.IsCorrect);

            return correct == null ? "A" : correct.OptionLabel;
        }

        // ==========================================
        // 13. HAM CAP NHAT HOAC THEM DAP AN
        // ==========================================
        private void SaveQuestionOption(
            VocabMasterDBEntities db,
            System.Collections.Generic.List<QuestionOption> options,
            int questionId,
            string label,
            string text,
            string correctOption)
        {
            var option = options.FirstOrDefault(
                o => o.OptionLabel == label);

            if (option == null)
            {
                option = new QuestionOption
                {
                    QuestionId = questionId,
                    OptionLabel = label
                };

                db.QuestionOptions.Add(option);
                options.Add(option);
            }

            option.OptionText = text.Trim();
            option.IsCorrect = correctOption == label;
        }

        // ==========================================
        // 14. NAP DANH SACH CHU DE
        // ==========================================
        private void LoadTopics()
        {
            using (var db = new VocabMasterDBEntities())
            {
                ViewBag.Topics = db.Topics
                    .OrderBy(t => t.TopicName)
                    .ToList();
            }
        }
    }
}
