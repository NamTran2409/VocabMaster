
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminVocabularyController : Controller
    {
        // DANH SACH TU VUNG
        public ActionResult Index(string searchString, int? topicId)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var vocabularies = db.Vocabularies
                    .Include(v => v.Topic)
                    .AsQueryable();

                // Tim kiem theo tu tieng Anh hoac nghia tieng Viet
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim();

                    vocabularies = vocabularies.Where(v =>
                        v.EnglishWord.Contains(searchString) ||
                        v.VietnameseMeaning.Contains(searchString));
                }

                // Loc theo chu de
                if (topicId.HasValue)
                {
                    vocabularies = vocabularies.Where(v =>
                        v.TopicId == topicId.Value);
                }

                var vocabularyList = vocabularies
                    .OrderBy(v => v.EnglishWord)
                    .ToList();

                // Danh sach chu de cho bo loc
                ViewBag.Topics = db.Topics
                    .OrderBy(t => t.TopicName)
                    .ToList();
                ViewBag.TopicCount = db.Topics.Count();

                ViewBag.SearchString = searchString;
                ViewBag.SelectedTopicId = topicId;

                return View(vocabularyList);
            }
        }
    }
}
