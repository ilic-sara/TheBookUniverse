using Models.Admin;

namespace Models.Books
{
    public class BookPageModel
    {
        public List<BookModel> Books { get; set; } = [];
        public List<FilterOptionsModel> FilterOptions { get; set; } = [];
        public List<string> SelectedGenres { get; set; } = [];
        public List<string> SelectedLanguages { get; set; } = []    ;
        public string SearchQuery { get; set; } = string.Empty;
        public long TotalNumberOfBooks { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public int? PriceFrom { get; set; }
        public int? PriceTo { get; set; }
        public int NumberOfBooksToDisplayPerPage { get; set; } = 5;
        public int CurrentPage { get; set; } = 1;
    }
}
