
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace VocabMaster.Models
{
    public class VocabularyCreateViewModel
    {
        public int VocabularyId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập từ tiếng Anh.")]
        [StringLength(150)]
        public string EnglishWord { get; set; }

        [StringLength(50)]
        public string PartOfSpeech { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nghĩa tiếng Việt.")]
        [StringLength(500)]
        public string VietnameseMeaning { get; set; }

        [StringLength(255)]
        public string Pronunciation { get; set; }

        [StringLength(1000)]
        public string ExampleSentence { get; set; }

        [StringLength(1000)]
        public string ExampleMeaning { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chủ đề.")]
        public int TopicId { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        public HttpPostedFileBase AudioFile { get; set; }

        // Lưu đường dẫn ảnh và âm thanh hiện tại
        public string CurrentImageUrl { get; set; }

        public string CurrentAudioUrl { get; set; }

        // Đánh dấu nếu muốn xóa ảnh hoặc âm thanh cũ
        public bool RemoveImage { get; set; }

        public bool RemoveAudio { get; set; }
    }
}
