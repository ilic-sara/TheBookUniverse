using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Admin;
using Models.Authors;
using Models.Books;
using Models.Shared;
using Services;

namespace TheBookUniverse.Areas.Common.Controllers
{
    [Area("Common")]
    public class HomeController : Controller
    {
        private readonly IAuthorService authorService;
        private readonly IBookService bookService;
        private readonly IUserService userService;
        private readonly IOrderService orderService;
        private readonly IBannerImageService bannerImageService;
        private readonly IFilterOptionsService filterOptionsService;
        private readonly UserManager<Data.User> userManager;
        private readonly ILogger<HomeController> logger;
        public HomeController(IAuthorService authorService,
                               IBookService bookService,
                               IUserService userService,
                               IOrderService orderService,
                               IBannerImageService bannerImageService,
                               IFilterOptionsService filterOptionsService,
                               UserManager<Data.User> userManager,
                               ILogger<HomeController> logger)
        {
            this.authorService = authorService;
            this.bookService = bookService;
            this.userService = userService;
            this.orderService = orderService;
            this.bannerImageService = bannerImageService;
            this.filterOptionsService = filterOptionsService;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<ActionResult> ViewStartPage()
        {
            StartPageModel model = new();

            try
            {
                List<BannerImageModel> bannerImages = await bannerImageService.GetAllAsync();
                if (bannerImages is null)
                    return NotFound();

                model.BannerImages = bannerImages;
                List<string> genresToDisplay = ["Romance", "Fantasy", "Classics", "Lifestyle", "Cooking"];
                foreach(var genre in genresToDisplay)
                {
                    List<BookModel> books = await bookService.GetBooksByOneGenreAsync(genre, 10);
                    if (books is not null)
                        model.BooksPerGenres.Add(genre, books);
                }

                return View(model);
            }
            catch (Exception ex)
            {

                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] ViewStartPage :: An error occured while fetching data to display start page.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> ViewAuthors(int startIndex = 1, int numberOfObjects = 24)
        {
            AuthorsPageModel model = new();

            try
            {
                List<AuthorModel> list = await authorService.PaginationSearchAsync(startIndex - 1, model.NumberOfAuthorsToDisplayPerPage);
                if (list is null)
                    return NotFound();

                model.Authors = list;
                model.CurrentPage = startIndex;
                model.TotalNumberOfAuthors = await authorService.GetTotalNumberOfAuthors();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] ViewAuthors :: An error occured while fetching authors with start index {startIndex}" +
                                    $" and number of authors to display {numberOfObjects}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> ViewAuthorDetails(string id)
        {
            try
            {
                AuthorModel model = await authorService.GetOneByIdWithBooksAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] ViewAuthorDetails :: An error occured while fetching author with id {id}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> GetBooksToDisplay(int startIndex = 1, int numberOfObjects = 24)
        {
            BookPageModel model = new();

            try
            {
                List<BookModel> list = await bookService.PaginationSearchAsync(startIndex - 1, model.NumberOfBooksToDisplayPerPage);
                if (list is null)
                    return NotFound();

                model.Books = list;
                model.FilterOptions = await filterOptionsService.GetAllAsync();
                if (model.FilterOptions is null)
                    return NotFound();

                model.TotalNumberOfBooks = await bookService.GetTotalNumberOfBooks();
                model.NumberOfBooksToDisplayPerPage = numberOfObjects;
                model.CurrentPage = startIndex;

                return View("ViewBooks", model);

            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] GetBooksToDisplay :: An error occured while fetching books with start index {startIndex}" +
                                    $" and number of books to display {numberOfObjects}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public ActionResult ViewBooks(BookPageModel model)
        {
            if (model is null)
                return NotFound();

            return View(model);
        }

        public async Task<ActionResult> ViewBookDetails(string id)
        {
            try
            {
                BookModel model = await bookService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] ViewBookDetails :: An error occured while fetching a book with id {id}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public ActionResult ErrorView()
        {
            return View();
        }

        public async Task<ActionResult> SearchAuthors(string searchQuery, int startIndex = 1, int numberOfObjects = 24)
        {
            try
            {
                List<AuthorModel> list = await authorService.GetAuthorsByPartialNameAsync(searchQuery, startIndex - 1, numberOfObjects);
                if (list is null)
                    return NotFound();

                AuthorsPageModel model = new();
                model.SearchQuery = searchQuery;
                model.Authors = list;
                model.CurrentPage = startIndex;
                model.TotalNumberOfAuthors = await authorService.GetNumberOfAuthorsByPartialName(searchQuery);
                return View("ViewAuthors", model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] SearchAuthors :: An error occured while fetching authors whose name contains {searchQuery}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> FilterBooks(BookPageModel model, int startIndex = 1)
        {
            try
            {
                int yearFrom, yearTo, priceFrom, priceTo;
                yearFrom = (int)(model.YearFrom is null ? 0 : model.YearFrom);
                yearTo = (int)(model.YearTo is null || model.YearTo == 0 ? DateTime.Now.Year : model.YearTo);
                priceFrom = (int)(model.PriceFrom is null ? 0 : model.PriceFrom);
                priceTo = (int)(model.PriceTo is null || model.PriceTo == 0 ? 1000 : model.PriceTo);

                List<BookModel> list = await bookService.GetFilteredBooksAsync(model.SearchQuery, 
                                                                               model.SelectedGenres, 
                                                                               model.SelectedLanguages, 
                                                                               yearFrom, 
                                                                               yearTo, 
                                                                               priceFrom, 
                                                                               priceTo, 
                                                                               startIndex - 1, 
                                                                               model.NumberOfBooksToDisplayPerPage);
                if (list is null)
                    return NotFound();

                model.FilterOptions = await filterOptionsService.GetAllAsync();
                if (model.FilterOptions is null)
                    return NotFound();

                model.CurrentPage = startIndex;
                model.TotalNumberOfBooks = await bookService.GetTotalNumberOfFilteredBooks(model.SearchQuery, 
                                                                                           model.SelectedGenres, 
                                                                                           model.SelectedLanguages, 
                                                                                           yearFrom, 
                                                                                           yearTo, 
                                                                                           priceFrom, 
                                                                                           priceTo);

                model.Books = list;
                return View("ViewBooks", model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] FilterBooks :: An error occured while filtering books.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

    }
}
