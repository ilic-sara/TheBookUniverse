using Models.Shared;

namespace Models.Books
{
    public class FavoriteBooksModel : BaseModel
    {
        public List<BookModel> Books { get; set; } = [];

    }
}
