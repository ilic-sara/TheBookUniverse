using Models.Books;

namespace Models.Orders
{
    public class CartItemModel
    {
        public BookModel Book { get; set; } = new();

        public int Quantity { get; set; }
    }
}
