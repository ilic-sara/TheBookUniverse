using Models.Admin;
using Models.Books;

namespace Models.Shared
{
    public class StartPageModel
    {
        public List<BannerImageModel> BannerImages { get; set; } = [];

        public Dictionary<string, List<BookModel>> BooksPerGenres { get; set; } = [];
    }
}
