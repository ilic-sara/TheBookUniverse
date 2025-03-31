using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.Users
{
    public class UserModel : BaseModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First name", Prompt = "Enter first name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last name", Prompt = "Enter last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [Display(Name = "Email", Prompt = "Enter email address")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserName is required")]
        [Display(Name = "Username", Prompt = "Enter username")]
        public string UserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string ComparePassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address", Prompt = "Enter address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City", Prompt = "Enter city")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal code", Prompt = "Enter postal code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country", Prompt = "Enter country")]
        public string Country { get; set; } = string.Empty;

    }
}
