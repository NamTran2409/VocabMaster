using System.Linq;
using System.Web.Mvc;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new VocabMasterDBEntities())
            {
                ViewBag.TopicCount = db.Topics.Count();
                ViewBag.VocabularyCount = db.Vocabularies.Count();
                ViewBag.QuestionCount = db.Questions.Count();
                ViewBag.UserCount = db.Users.Count();
            }

            return View();
        }
    }
}