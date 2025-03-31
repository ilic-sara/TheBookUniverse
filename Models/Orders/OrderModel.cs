using System.ComponentModel.DataAnnotations;
using Models.Shared;

namespace Models.Orders
{
    public class OrderModel : BaseModel
    {
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only letters are allowed.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only letters are allowed.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        [RegularExpression(@"^\d{5}$", ErrorMessage = "Invalid ZIP code")]
        [Display(Name = "Postal code")]
        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public List<CartItemModel> Items { get; set; } = [];

        public decimal Price { get; set; }

        [Display(Name = "Date created")]
        public DateTime DateCreated { get; set; }

        public string UserBoughtId { get; set; } = string.Empty;

        public bool Sent { get; set; }

    }
}
