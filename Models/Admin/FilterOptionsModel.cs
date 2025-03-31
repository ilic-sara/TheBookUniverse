using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.Admin
{
    public class FilterOptionsModel : BaseModel 
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name", Prompt = "Enter name")]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessage = "Values are required")]
        [Display(Name = "Values", Prompt = "Enter values")]
        public List<string> Values { get; set; } = [];
    }
}
