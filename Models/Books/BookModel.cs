using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.Books
{
    public class BookModel : BaseModel
    {
        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Title", Prompt = "Enter book title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description", Prompt = "Enter book description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Published year field is required")]
        [Display(Name = "Published year", Prompt = "Enter published year")]
        public int PublishedYear { get; set; }

        [Required(ErrorMessage = "Genres selection is required")]
        [Display(Name = "Genres", Prompt = "Select genres")]
        public List<string> Genres { get; set; } = [];

        [Required(ErrorMessage = "Author selection is required")]
        [Display(Name = "Author", Prompt = "Select author")]
        public string AuthorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author selection is required")]
        [Display(Name = "Author", Prompt = "Select author")]
        public string AuthorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Price", Prompt = "Enter book price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Number of pages field is required")]
        [Display(Name = "Number of pages", Prompt = "Enter number of pages")]
        public int NumberOfPages { get; set; }

        [Required(ErrorMessage = "Inventory is required")]
        [Display(Name = "Inventory", Prompt = "Enter book inventory")]
        public int Inventory { get; set; }

        [Required(ErrorMessage = "Picture URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Picture URL", Prompt = "Enter picture URL")]
        public string PictureURL { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required")]
        [Display(Name = "Language", Prompt = "Select language")]
        public string Language { get; set; } = string.Empty;

    }
}
