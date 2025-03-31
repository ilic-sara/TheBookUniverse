using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.Books
{
    public class BookCommentModel : BaseModel
    {
        [Required(ErrorMessage = "Comment text is required")]
        [Display(Name = "Comment text", Prompt = "Enter Comment text")]
        public string CommentText { get; set; } = string.Empty;

        public string User_Username { get; set; } = string.Empty;

        public DateTime TimePosted { get; set; }

        public string User_Id { get; set; } = string.Empty;
    }
}
