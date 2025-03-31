using System.ComponentModel.DataAnnotations;
using Models.Books;
using Models.Shared;

namespace Models.Authors
{
    public class AuthorModel : BaseModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name", Prompt = "Enter name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author information is required")]
        [Display(Name = "About", Prompt = "Enter author information")]
        public string About { get; set; } = string.Empty;

        [Required(ErrorMessage = "Picture URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Picture URL", Prompt = "Enter picture URL")]
        public string PictureURL { get; set; } = string.Empty;
        public List<BookModel> Books { get; set; } = [];

    }
}
