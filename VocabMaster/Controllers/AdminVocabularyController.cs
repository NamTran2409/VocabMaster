
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using VocabMaster.Models;
using System.IO;

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

                // Tim kiem theo tu tieng Anh, nghia tieng Viet hoac ten chu de
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim();

                    vocabularies = vocabularies.Where(v =>
                        v.EnglishWord.Contains(searchString) ||
                        v.VietnameseMeaning.Contains(searchString) ||
                        v.Topic.TopicName.Contains(searchString));
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



        // HIEN THI FORM THEM TU VUNG
        [HttpGet]
        public ActionResult Create()
        {
            using (var db = new VocabMasterDBEntities())
            {
                ViewBag.Topics = db.Topics
                    .Where(t => t.Status == "Published")
                    .OrderBy(t => t.TopicName)
                    .ToList();

                return View(new VocabularyCreateViewModel());
            }
        }

        // XU LY THEM TU VUNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VocabularyCreateViewModel model)
        {
            using (var db = new VocabMasterDBEntities())
            {
                // Kiem tra chu de
                bool topicExists = db.Topics.Any(t =>
                    t.TopicId == model.TopicId &&
                    t.Status == "Published");

                if (!topicExists)
                {
                    ModelState.AddModelError(
                        "TopicId",
                        "Vui long chon chu de hop le.");
                }

                // Kiem tra anh
                if (model.ImageFile != null &&
                    model.ImageFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(
                        model.ImageFile.FileName).ToLower();

                    string[] allowedImages =
                    {
                        ".jpg", ".jpeg", ".png", ".webp"
                    };

                    if (!allowedImages.Contains(extension))
                    {
                        ModelState.AddModelError(
                            "ImageFile",
                            "Anh phai co dinh dang JPG, JPEG, PNG hoac WEBP.");
                    }

                    if (model.ImageFile.ContentLength > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "ImageFile",
                            "Kich thuoc anh khong duoc vuot qua 2MB.");
                    }
                }

                // Kiem tra am thanh
                if (model.AudioFile != null &&
                    model.AudioFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(
                        model.AudioFile.FileName).ToLower();

                    string[] allowedAudio =
                    {
                        ".mp3", ".wav", ".ogg"
                    };

                    if (!allowedAudio.Contains(extension))
                    {
                        ModelState.AddModelError(
                            "AudioFile",
                            "Am thanh phai co dinh dang MP3, WAV hoac OGG.");
                    }

                    if (model.AudioFile.ContentLength > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "AudioFile",
                            "Kich thuoc am thanh khong duoc vuot qua 10MB.");
                    }
                }

                // Neu du lieu khong hop le
                if (!ModelState.IsValid)
                {
                    ViewBag.Topics = db.Topics
                        .Where(t => t.Status == "Published")
                        .OrderBy(t => t.TopicName)
                        .ToList();

                    return View(model);
                }

                string imageUrl = null;
                string audioUrl = null;

                // Luu anh
                if (model.ImageFile != null &&
                    model.ImageFile.ContentLength > 0)
                {
                    string folder = Server.MapPath(
                        "~/Content/Uploads/Vocabulary/Images");

                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(model.ImageFile.FileName);

                    model.ImageFile.SaveAs(
                        Path.Combine(folder, fileName));

                    imageUrl = "/Content/Uploads/Vocabulary/Images/"
                        + fileName;
                }

                // Luu am thanh
                if (model.AudioFile != null &&
                    model.AudioFile.ContentLength > 0)
                {
                    string folder = Server.MapPath(
                        "~/Content/Uploads/Vocabulary/Audio");

                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(model.AudioFile.FileName);

                    model.AudioFile.SaveAs(
                        Path.Combine(folder, fileName));

                    audioUrl = "/Content/Uploads/Vocabulary/Audio/"
                        + fileName;
                }

                // Tao doi tuong Vocabulary
                var vocabulary = new Vocabulary
                {
                    TopicId = model.TopicId,
                    EnglishWord = model.EnglishWord.Trim(),
                    PartOfSpeech = model.PartOfSpeech,
                    VietnameseMeaning = model.VietnameseMeaning.Trim(),
                    Pronunciation = model.Pronunciation,
                    ExampleSentence = model.ExampleSentence,
                    ExampleMeaning = model.ExampleMeaning,
                    ImageUrl = imageUrl,
                    AudioUrl = audioUrl,
                    Status = "Published",
                    CreatedAt = DateTime.Now
                };

                db.Vocabularies.Add(vocabulary);
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Them tu vung thanh cong!";
            }

            return RedirectToAction("Index");
        }


    }
}
