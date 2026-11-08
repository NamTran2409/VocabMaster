
using System;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.IO;
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

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.Trim();

                    vocabularies = vocabularies.Where(v =>
                        v.EnglishWord.Contains(searchString) ||
                        v.VietnameseMeaning.Contains(searchString) ||
                        v.Topic.TopicName.Contains(searchString));
                }

                if (topicId.HasValue)
                {
                    vocabularies = vocabularies.Where(v =>
                        v.TopicId == topicId.Value);
                }

                var vocabularyList = vocabularies
                    .OrderBy(v => v.EnglishWord)
                    .ToList();

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
                bool topicExists = db.Topics.Any(t =>
                    t.TopicId == model.TopicId &&
                    t.Status == "Published");

                if (!topicExists)
                {
                    ModelState.AddModelError(
                        "TopicId",
                        "Vui long chon chu de hop le.");
                }

                ValidateUploadedFiles(model.ImageFile, model.AudioFile);

                if (!ModelState.IsValid)
                {
                    ViewBag.Topics = db.Topics
                        .Where(t => t.Status == "Published")
                        .OrderBy(t => t.TopicName)
                        .ToList();

                    return View(model);
                }

                string imageUrl = SaveUploadedFile(
                    model.ImageFile,
                    "~/Content/Uploads/Vocabulary/Images");

                string audioUrl = SaveUploadedFile(
                    model.AudioFile,
                    "~/Content/Uploads/Vocabulary/Audio");

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

                TempData["SuccessMessage"] = "Them tu vung thanh cong!";
            }

            return RedirectToAction("Index");
        }

        // HIEN THI FORM CHINH SUA
        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var vocabulary = db.Vocabularies.Find(id);

                if (vocabulary == null)
                {
                    return HttpNotFound();
                }

                LoadPublishedTopics(db);

                var model = new VocabularyCreateViewModel
                {
                    VocabularyId = vocabulary.VocabularyId,
                    TopicId = vocabulary.TopicId,
                    EnglishWord = vocabulary.EnglishWord,
                    PartOfSpeech = vocabulary.PartOfSpeech,
                    VietnameseMeaning = vocabulary.VietnameseMeaning,
                    Pronunciation = vocabulary.Pronunciation,
                    ExampleSentence = vocabulary.ExampleSentence,
                    ExampleMeaning = vocabulary.ExampleMeaning,
                    CurrentImageUrl = vocabulary.ImageUrl,
                    CurrentAudioUrl = vocabulary.AudioUrl
                };

                return View(model);
            }
        }

        // XU LY CAP NHAT TU VUNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VocabularyCreateViewModel model)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var vocabulary = db.Vocabularies.Find(model.VocabularyId);

                if (vocabulary == null)
                {
                    return HttpNotFound();
                }

                bool topicExists = db.Topics.Any(t =>
                    t.TopicId == model.TopicId &&
                    t.Status == "Published");

                if (!topicExists)
                {
                    ModelState.AddModelError(
                        "TopicId",
                        "Vui long chon chu de hop le.");
                }

                ValidateUploadedFiles(model.ImageFile, model.AudioFile);

                if (!ModelState.IsValid)
                {
                    LoadPublishedTopics(db);

                    model.CurrentImageUrl = vocabulary.ImageUrl;
                    model.CurrentAudioUrl = vocabulary.AudioUrl;

                    return View(model);
                }

                string oldImageUrl = vocabulary.ImageUrl;
                string oldAudioUrl = vocabulary.AudioUrl;

                string newImageUrl = model.RemoveImage
                    ? null
                    : vocabulary.ImageUrl;

                string newAudioUrl = model.RemoveAudio
                    ? null
                    : vocabulary.AudioUrl;

                if (model.ImageFile != null &&
                    model.ImageFile.ContentLength > 0)
                {
                    newImageUrl = SaveUploadedFile(
                        model.ImageFile,
                        "~/Content/Uploads/Vocabulary/Images");
                }

                if (model.AudioFile != null &&
                    model.AudioFile.ContentLength > 0)
                {
                    newAudioUrl = SaveUploadedFile(
                        model.AudioFile,
                        "~/Content/Uploads/Vocabulary/Audio");
                }

                vocabulary.TopicId = model.TopicId;
                vocabulary.EnglishWord = model.EnglishWord.Trim();
                vocabulary.PartOfSpeech = model.PartOfSpeech;
                vocabulary.VietnameseMeaning = model.VietnameseMeaning.Trim();
                vocabulary.Pronunciation = model.Pronunciation;
                vocabulary.ExampleSentence = model.ExampleSentence;
                vocabulary.ExampleMeaning = model.ExampleMeaning;
                vocabulary.ImageUrl = newImageUrl;
                vocabulary.AudioUrl = newAudioUrl;

                db.SaveChanges();

                if (oldImageUrl != newImageUrl)
                {
                    DeleteUploadedFile(
                        oldImageUrl,
                        "~/Content/Uploads/Vocabulary/Images");
                }

                if (oldAudioUrl != newAudioUrl)
                {
                    DeleteUploadedFile(
                        oldAudioUrl,
                        "~/Content/Uploads/Vocabulary/Audio");
                }

                TempData["SuccessMessage"] =
                    "Cap nhat tu vung thanh cong!";
            }

            return RedirectToAction("Index");
        }

        // XOA TU VUNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var db = new VocabMasterDBEntities())
            {
                var vocabulary = db.Vocabularies
                    .Include(v => v.UserVocabularyProgresses)
                    .FirstOrDefault(v => v.VocabularyId == id);

                if (vocabulary == null)
                {
                    return HttpNotFound();
                }

                // KIEM TRA DU LIEU HOC TAP LIEN QUAN
                if (vocabulary.UserVocabularyProgresses.Any())
                {
                    TempData["ErrorMessage"] =
                        "Khong the xoa tu vung nay vi da co du lieu hoc tap lien quan.";

                    return RedirectToAction("Index");
                }

                string imageUrl = vocabulary.ImageUrl;
                string audioUrl = vocabulary.AudioUrl;

                // XOA TU VUNG TRONG DATABASE
                db.Vocabularies.Remove(vocabulary);
                db.SaveChanges();

                // XOA ANH VA AM THANH SAU KHI XOA DATABASE THANH CONG
                DeleteUploadedFile(
                    imageUrl,
                    "~/Content/Uploads/Vocabulary/Images");

                DeleteUploadedFile(
                    audioUrl,
                    "~/Content/Uploads/Vocabulary/Audio");

                TempData["SuccessMessage"] =
                    "Xoa tu vung thanh cong!";
            }

            return RedirectToAction("Index");
        }

        // NAP DANH SACH CHU DE
        private void LoadPublishedTopics(VocabMasterDBEntities db)
        {
            ViewBag.Topics = db.Topics
                .Where(t => t.Status == "Published")
                .OrderBy(t => t.TopicName)
                .ToList();
        }

        // KIEM TRA DINH DANG VA DUNG LUONG FILE
        private void ValidateUploadedFiles(
            HttpPostedFileBase imageFile,
            HttpPostedFileBase audioFile)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(
                    imageFile.FileName).ToLowerInvariant();

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

                if (imageFile.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Kich thuoc anh khong duoc vuot qua 2MB.");
                }
            }

            if (audioFile != null && audioFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(
                    audioFile.FileName).ToLowerInvariant();

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

                if (audioFile.ContentLength > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "AudioFile",
                        "Kich thuoc am thanh khong duoc vuot qua 10MB.");
                }
            }
        }

        // LUU FILE VAO THU MUC UPLOAD
        private string SaveUploadedFile(
            HttpPostedFileBase file,
            string virtualFolder)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }

            string folder = Server.MapPath(virtualFolder);
            Directory.CreateDirectory(folder);

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string fileName = Guid.NewGuid().ToString("N") + extension;

            file.SaveAs(Path.Combine(folder, fileName));

            string folderUrl = virtualFolder
                .Replace("~", "")
                .Replace("\\", "/");

            return folderUrl.TrimEnd('/') + "/" + fileName;
        }

        // XOA FILE CU TRONG THU MUC UPLOAD
        private void DeleteUploadedFile(
            string fileUrl,
            string expectedVirtualFolder)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return;
            }

            string expectedUrlPrefix = expectedVirtualFolder
                .Replace("~", "")
                .Replace("\\", "/")
                .TrimEnd('/') + "/";

            if (!fileUrl.StartsWith(
                expectedUrlPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fileName = Path.GetFileName(fileUrl);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            string folder = Server.MapPath(expectedVirtualFolder);
            string filePath = Path.Combine(folder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
