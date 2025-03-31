using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.Admin
{
    public class BannerImageModel : BaseModel
    {
        [Required(ErrorMessage = "Picture URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Picture URL", Prompt = "Enter picture URL")]
        public string PictureURL { get; set; } = string.Empty;
    }
}
