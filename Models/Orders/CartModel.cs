using Models.Shared;

namespace Models.Orders
{
    public class CartModel : BaseModel
    {
        public List<CartItemModel> CartItems { get; set; } = [];

    }
}
