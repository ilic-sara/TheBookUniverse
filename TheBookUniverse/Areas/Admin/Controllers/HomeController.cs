using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Admin;
using Models.Authors;
using Models.Books;
using Models.Orders;
using Services;

namespace TheBookUniverse.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

        private void PostTextResponse (bool successfull, string message)
        {
            TempData["Response"] = successfull;
            TempData["ResponseMessage"] = message;
        }

        #region Insert

        public async Task<ActionResult> AddBook()
        {
            try
            {
                var authors = await authorService.GetNamesOfAllAuthorsAsync();
                if (authors is null)
                    return NotFound();

                var genres = await filterOptionsService.GetValuesByNameAsync("Genres");
                if (genres is null)
                    return NotFound();

                var languages = await filterOptionsService.GetValuesByNameAsync("Languages");
                if (languages is null)
                    return NotFound();

                ViewBag.Authors = authors;
                ViewBag.Genres = genres;
                ViewBag.Languages = languages;

                return View();
            }
            catch(Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] AddBook :: An error occured while fetching data to add new book.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddBook(BookModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var authors = await authorService.GetNamesOfAllAuthorsAsync();
                    if (authors is null)
                        return NotFound();

                    var genres = await filterOptionsService.GetValuesByNameAsync("Genres");
                    if (genres is null)
                        return NotFound();

                    var languages = await filterOptionsService.GetValuesByNameAsync("Languages");
                    if (languages is null)
                        return NotFound();

                    ViewBag.Authors = authors;
                    ViewBag.Genres = genres;
                    ViewBag.Languages = languages;

                    PostTextResponse(false, "Insert failed - incorrect data.");
                    return View(model);
                }
                string id = await bookService.InsertAsync(model);
                if(string.IsNullOrWhiteSpace(id))
                    return StatusCode(500, "Internal server error");
                PostTextResponse(true, "Book added successfully.");
                return RedirectToAction("ViewBookDetails", "Home", new { area = "Common", id = id });

            }
            catch (Exception ex)
            {
                PostTextResponse(false, "Insert failed.");
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AddBook :: An error occured while trying to add new book.\n{ex}");

                return StatusCode(500, "Internal server error");

            }
        }

        public ActionResult AddAuthor()
        {
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAuthor(AuthorModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostTextResponse(false, "Insert failed - incorrect data.");
                    return View(model);
                }
                string id = await authorService.InsertAsync(model);
                if(!string.IsNullOrWhiteSpace(id))
                {
                    PostTextResponse(true, "Author added successfully.");
                    return RedirectToAction("ViewAuthorDetails", "Home", new { area = "Common", id = id });
                }
                else
                    return StatusCode(500, "Internal server error");
            }
            catch(Exception ex)
            {
                PostTextResponse(false, "Insert failed.");
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AddAuthor :: An error occured while trying to add new author.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
            
        }

        public ActionResult AddBannerImage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddBannerImage(BannerImageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostTextResponse(false, "Insert failed - incorrect data.");
                    return View(model);
                }
                string id = await bannerImageService.InsertAsync(model);
                if (!string.IsNullOrWhiteSpace(id))
                    PostTextResponse(true, "Banner image added successfully.");
                else
                    PostTextResponse(false, "Insert failed.");
                return View(model);
            }
            catch (Exception ex)
            {
                PostTextResponse(false, "Insert failed.");
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AddBannerImage :: An error occured while trying to add new banner image.\n{ex}");
                return View(model);
            }
        }

        public ActionResult AddFilterOption()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddFilterOption(FilterOptionsModel model)
        {
            if (model.Values is null || model.Values.Count == 0)
            {
                PostTextResponse(false, "Error: You need to have selected values.");
                return View(model);
            }
            else
            {
                try
                {
                    model.Values.Sort();
                    string id = await filterOptionsService.InsertAsync(model);
                    if (!string.IsNullOrWhiteSpace(id))
                        PostTextResponse(true, "Filter option was sucessfully inserted.");
                    else
                        PostTextResponse(false, "Insert failed.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    PostTextResponse(false, "Insert failed.");
                    logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] AddFilterOption :: An error occured while trying to add new filter option.\n{ex}");
                    return View(model);
                }
            }
            
        }


        #endregion


        #region View

        public async Task<ActionResult> ViewAllOrders()
        {
            try
            {
                var list = await orderService.GetAllAsync();
                if (list is null)
                    return NotFound();

                ViewBag.Title = "All orders";
                return View("ViewOrders", list);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewAllOrders :: An error occured while fetching all orders.\n{ex}");
                throw;
            }
        }

        public ActionResult ViewOrders(List<OrderModel> list)
        {
            list = list.OrderBy(a => a.DateCreated).ToList();
            return View(list);
        }

        public async Task<ActionResult> ViewOrderDetails(string id)
        {
            try
            {
                OrderModel model = await orderService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewOrderDetails :: An error occured while fetching order with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<ActionResult> ViewBannerImages()
        {
            try
            {
                List<BannerImageModel> list = await bannerImageService.GetAllAsync();
                if (list is null)
                    return NotFound();

                return View(list);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewBannerImages :: An error occured while fetching all banner images.\n{ex}");
                throw;
            }
        }

        public async Task<ActionResult> ViewFilterOptions()
        {
            try
            {
                List<FilterOptionsModel> list = await filterOptionsService.GetAllAsync();
                if (list is null)
                    return NotFound();

                return View(list);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewFilterOptions :: An error occured while fetching all filter options.\n{ex}");
                throw;
            }
        }

        public async Task<ActionResult> ViewFilterOptionDetails(string id)
        {
            try
            {
                FilterOptionsModel model = await filterOptionsService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewFilterOptionDetails :: An error occured while fetching filter option with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<ActionResult> ShowFilteredOrders(bool isSent)
        {
            string sent = isSent ? "Sent orders" : "Not sent orders";
            try
            {
                List<OrderModel> model = await orderService.GetMultipleBySentValueAsync(isSent);
                if (model is null)
                    return NotFound();

                ViewBag.Title = sent;
                return View("ViewOrders", model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ShowFilteredOrders :: An error occured while fetching {sent}.\n{ex}");
                throw;
            }
        }

        #endregion


        #region Edit

        public async Task<ActionResult> EditBook(string id)
        {
            try
            {
                BookModel model = await bookService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                Dictionary<string,string> authors = await authorService.GetNamesOfAllAuthorsAsync();
                if (authors is null)
                    return NotFound();

                List<string> genres = await filterOptionsService.GetValuesByNameAsync("Genres");
                if (genres is null)
                    return NotFound();

                List<string> languages = await filterOptionsService.GetValuesByNameAsync("Languages");
                if (languages is null)
                    return NotFound();

                ViewBag.Authors = authors;
                ViewBag.Genres = genres;
                ViewBag.Languages = languages;

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditBook :: An error occured while fetching data to edit book with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBook(BookModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Dictionary<string, string> authors = await authorService.GetNamesOfAllAuthorsAsync();
                    if (authors is null)
                        return NotFound();

                    List<string> genres = await filterOptionsService.GetValuesByNameAsync("Genres");
                    if (genres is null)
                        return NotFound();

                    List<string> languages = await filterOptionsService.GetValuesByNameAsync("Languages");
                    if (languages is null)
                        return NotFound();

                    ViewBag.Authors = authors;
                    ViewBag.Genres = genres;
                    ViewBag.Languages = languages;

                    PostTextResponse(false, "Edit failed - incorrect data.");
                    return View(model);
                }
                await bookService.UpdateAsync(model);
                PostTextResponse(true, "Book was successfully updated.");
                return RedirectToAction("ViewBookDetails", "Home", new { area = "Common", id = model.Id });
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditBook :: An error occured while updating book with id {model.Id}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }


        public async Task<ActionResult> EditAuthor(string id)
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
                    $"[ERROR] EditAuthor :: An error occured while fetching author with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAuthor(AuthorModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostTextResponse(false, "Edit failed - incorrect data.");
                    return View(model);
                }
                await authorService.UpdateAsync(model);
                PostTextResponse(true, "Author was sucessfully updated.");
                return RedirectToAction("ViewAuthorDetails", "Home", new { area = "Common", id = model.Id });
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditAuthor :: An error occured while updating author with id {model.Id}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }


        public async Task<ActionResult> EditBannerImage(string id)
        {
            try
            {
                var model = await bannerImageService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditBannerImage :: An error occured while fetching banner image with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBannerImage(BannerImageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostTextResponse(false, "Edit failed - incorrect data.");
                    return View(model);
                }
                await bannerImageService.UpdateAsync(model);
                PostTextResponse(true, "Banner image was sucessfully updated.");
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditBannerImage :: An error occured while updating banner image with id {model.Id}.\n{ex}");
                PostTextResponse(false, "Edit failed.");
                return View(model);
            }
        }


        public async Task<ActionResult> EditFilterOption(string id)
        {
            try
            {
                var model = await filterOptionsService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditFilterOption :: An error occured while fetching filter option with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFilterOption(FilterOptionsModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostTextResponse(false, "Edit failed - incorrect data.");
                    return View(model);
                }
                model.Values.Sort();
                await filterOptionsService.UpdateAsync(model);
                PostTextResponse(true, "Filter option was sucessfully updated.");
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditFilterOption :: An error occured while updating filter option with id {model.Id}.\n{ex}");
                PostTextResponse(false, "Edit failed.");
                return View(model);
            }
        }

        public async Task<ActionResult> MarkAsSent(string id)
        {
            try
            {
                await orderService.MarkAsSentAsync(id);
                OrderModel model = await orderService.GetOneByIdAsync(id);
                if (model is null)
                {
                    return NotFound();
                }
                return View("ViewOrderDetails", model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] MarkAsSent :: An error occured while updating order with id {id} to sent.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion


        #region Delete

        public async Task<ActionResult> DeleteBook(string id)
        {
            try
            {
                var model = await bookService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteBook :: An error occured while fetching book with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteBookAction(string id)
        {
            try
            {
                await bookService.DeleteAsync(id);

                return RedirectToAction("GetBooksToDisplay", "Home", new { area = "Common" });
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteBookAction :: An error occured while deleting book with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");

            }
        }

        public async Task<ActionResult> DeleteAuthor(string id)
        {
            try
            {
                var model = await authorService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAuthor :: An error occured while fetching author with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAuthorAction(string id)
        {
            try
            {
                await authorService.DeleteAsync(id);

                return RedirectToAction("ViewAuthors", "Home", new { area = "Common" });
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAuthorAction :: An error occured while deleting author with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");

            }
        }

        public async Task<ActionResult> DeleteFilterOption(string id)
        {
            try
            {
                var model = await filterOptionsService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteFilterOption :: An error occured while fetching filter option with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteFilterOptionAction(string id)
        {
            try
            {
                await filterOptionsService.DeleteAsync(id);

                return RedirectToAction("ViewFilterOptions");
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteFilterOptionAction :: An error occured while deleting filter option with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");

            }
        }

        public async Task<ActionResult> DeleteBannerImage(string id)
        {
            try
            {
                var model = await bannerImageService.GetOneByIdAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteBannerImage :: An error occured while fetching banner image with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteBannerImageAction(string id)
        {
            try
            {
                await bannerImageService.DeleteAsync(id);

                return RedirectToAction("ViewBannerImages");
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteBannerImageAction :: An error occured while deleting banner image with id {id}.\n{ex}");
                return StatusCode(500, "Internal server error");

            }
        }
        #endregion
    }
}
