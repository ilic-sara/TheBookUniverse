namespace Models.Authors
{
    public class AuthorsPageModel
    {
        public List<AuthorModel> Authors { get; set; } = [];
        public string SearchQuery { get; set; } = string.Empty;
        public long TotalNumberOfAuthors { get; set; }
        public int NumberOfAuthorsToDisplayPerPage { get; set; } = 12;
        public int CurrentPage { get; set; } = 1;
    }
}
