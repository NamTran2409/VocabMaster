
using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Models
{
    public class QuestionCreateViewModel
    {
        public int QuestionId { get; set; }

        public int QuizId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi.")]
        [StringLength(1000)]
        public string QuestionText { get; set; }

        [StringLength(2000)]
        public string Explanation { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án A.")]
        [StringLength(500)]
        public string OptionA { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án B.")]
        [StringLength(500)]
        public string OptionB { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án C.")]
        [StringLength(500)]
        public string OptionC { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án D.")]
        [StringLength(500)]
        public string OptionD { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đáp án đúng.")]
        public string CorrectOption { get; set; }
    }
}
