using AutoMapper;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Books;
using Models.Orders;
using Models.Users;
using Services;

namespace TheBookUniverse.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]
    public class HomeController : Controller
    {
        private readonly IAuthorService authorService;
        private readonly IBookService bookService;
        private readonly IUserService userService;
        private readonly IOrderService orderService;
        private readonly UserManager<Data.User> userManager;
        private readonly IMapper mapper;
        private readonly ILogger<HomeController> logger;

        public HomeController(IAuthorService authorService,
                               IBookService bookService,
                               IUserService userService,
                               IOrderService orderService,
                               UserManager<Data.User> userManager,
                               IMapper mapper,
                               ILogger<HomeController> logger)
        {
            this.authorService = authorService;
            this.bookService = bookService;
            this.userService = userService;
            this.orderService = orderService;
            this.userManager = userManager;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<ActionResult> ViewMyProfile()
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    return NotFound();

                MyProfileModel model = mapper.Map<MyProfileModel>(user);
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewMyProfile :: An error occured while fetching data to display user profile.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> ViewMyCart()
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    return NotFound();

                CartModel model = new CartModel();
                model.CartItems = await userService.GetCartItemModelsForUser(user.Id);
               
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewMyCart :: An error occured while fetching data to display user cart.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookAmount(string bookId, int newValue, decimal price)
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null || user.CartItems is null)
                    return NotFound();

                CartItem? itemToUpdate = user.CartItems.Where(a => a.BookId == bookId).FirstOrDefault();
                if (itemToUpdate is null)
                    return NotFound();

                if (newValue != itemToUpdate.Quantity)
                {
                    itemToUpdate.Quantity = newValue;
                    await userService.UpdateAsync(user);
                }
                decimal total = await userService.GetSumOfItemsInCartForUser(user.Id);
                return Json(total);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateBookAmount :: An error occured while trying to update number of copies of a book with id {bookId} in cart.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> AddBookToCart(string id, int numberOfCopies = 1)
        {
            try
            {
                string refererUrl = Request.Headers["Referer"].ToString();

                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    return NotFound();

                if (user.CartItems is null)
                {
                    user.CartItems = new()
                    {
                        new() { BookId = id, Quantity = numberOfCopies}
                    };
                }
                else
                {
                    CartItem? item = user.CartItems.FirstOrDefault(a => a.BookId == id);
                    if (item is null)
                    {
                        item = new CartItem() { BookId = id, Quantity = numberOfCopies };
                        user.CartItems.Add(item);
                    }
                    else
                        item.Quantity += numberOfCopies;
                }
                
                await userService.UpdateAsync(user);

                int numberOfItemsInCart = user.CartItems.Sum(x => x.Quantity);
                TempData["NumberOfItemsInCart"] = numberOfItemsInCart;

                return Redirect(refererUrl);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AddBookToCart :: An error occured while trying to add new book to cart.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> AddBookToFavorites(string id)
        {
            try
            {
                string refererUrl = Request.Headers["Referer"].ToString();

                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    throw new Exception($"Current user not found.");

                if (user.FavoriteBooksIds is null)
                {
                    user.FavoriteBooksIds = new();
                }
                if (!user.FavoriteBooksIds.Contains(id))
                {
                    user.FavoriteBooksIds.Add(id);
                    await userService.UpdateAsync(user);
                }

                return Redirect(refererUrl);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AddBookToFavorites :: An error occured while trying to add new book to favorites.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> RemoveBookFromFavorites(string id)
        {
            try
            {
                string refererUrl = Request.Headers["Referer"].ToString();

                Data.User? user = await userManager.GetUserAsync(User);
                if(user is null)
                    throw new Exception($"Current user not found.");
                if (user.FavoriteBooksIds is null || user.FavoriteBooksIds.Count == 0 || !user.FavoriteBooksIds.Contains(id))
                    throw new Exception($"Book with id {id} should be removed from favorites for user with id {user.Id}, but it is not in the list.");

                user.FavoriteBooksIds.Remove(id);
                await userService.UpdateAsync(user);

                return Redirect(refererUrl);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] RemoveBookFromFavorites :: An error occured while removing a book from favorites.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> RemoveBookFromCart(string id)
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null || user.CartItems is null || !user.CartItems.Any(x => x.BookId == id))
                    return NotFound();

                CartItem? itemToRemove = user.CartItems.Where(x => x.BookId == id).FirstOrDefault();
                if (itemToRemove is null)
                    throw new Exception("Something went wrong - cart item not found.");
                user.CartItems.Remove(itemToRemove);
                await userService.UpdateAsync(user);

                return RedirectToAction("ViewMyCart", "Home");
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] RemoveBookFromCart :: An error occured while removing a book from cart.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> ViewMyFavoriteBooks()
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    return NotFound();

                if (user.FavoriteBooksIds is null)
                {
                    var emptyModel = new FavoriteBooksModel()
                    {
                        Books = new()
                    };
                    return View(emptyModel);

                }
                var books = await bookService.GetMultipleByIdsAsync(user.FavoriteBooksIds);
                if (books is null)
                    return NotFound();

                var bookModels = mapper.Map<List<BookModel>>(books);
                var model = new FavoriteBooksModel()
                {
                    Books = bookModels
                };
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewMyFavoriteBooks :: An error occured while fetching favorite books for a user.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> ViewMyOrders()
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    return NotFound();

                if (user.OrdersIds is null)
                {
                    return View(new List<OrderModel>());
                }

                List<OrderModel> orders = await orderService.GetMultipleByIdsAsync(user.OrdersIds);
                if (orders is null)
                    return NotFound();

                orders = orders.OrderByDescending(o => o.DateCreated).ToList();
               
                return View(orders);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] ViewMyOrders :: An error occured while fetching user orders.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> EditMyProfile()
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                    return NotFound();

                var model = mapper.Map<MyProfileModel>(user);
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditMyProfile :: An error occured while fetching data to edit user profile.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditMyProfile(MyProfileModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Update failed - incorrect data.";
                    return View(model);
                }

                Data.User? user = await userManager.GetUserAsync(User);
                if (user is null)
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Update failed.";
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Address = model.Address;
                user.City = model.City;
                user.PostalCode = model.PostalCode;
                user.Country = model.Country;
                user.NormalizedEmail = model.Email.ToUpperInvariant();
                user.NormalizedUserName = model.UserName.ToUpperInvariant();
                await userService.UpdateAsync(user);

                TempData["Response"] = true;
                TempData["ResponseMessage"] = "Successfully updated.";
                return View(model);

            }
            catch (Exception ex)
            {
                TempData["Response"] = false;
                TempData["ResponseMessage"] = "Update failed.";

                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] EditMyProfile :: An error occured while editing profile for user with id {model.Id}.\n{ex}");
                return View(model);
            }
        }
        public async Task<ActionResult> AddOrderInformation()
        {
            try
            {
                Data.User? user = await userManager.GetUserAsync(User);
                if (user is not null && user.CartItems is not null && user.CartItems.Count > 0)
                {
                    DateTime currentDateTime = DateTime.Now;
                    OrderModel order = new OrderModel()
                    {
                        Price = await userService.GetSumOfItemsInCartForUser(user.Id),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Address = user.Address,
                        City = user.City,
                        PostalCode = user.PostalCode,
                        Country = user.Country,
                        UserBoughtId = user.Id.ToString()

                    };

                    return View(order);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AddOrderInformation :: An error occured while creating new order.\n{ex}");

                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BuyBooks(OrderModel order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Incorrect data.";
                    return View("AddOrderInformation", order);
                }

                Data.User? user = await userManager.GetUserAsync(User);
                if(user is null)
                    throw new Exception($"Current user not found.");

                if(user.CartItems is null || user.CartItems.Count == 0)
                    throw new Exception($"User with id {user.Id} doesn't have any items in cart.");

                order.Items = mapper.Map<List<CartItemModel>>(user.CartItems);
                DateTime currentDateTime = DateTime.Now;
                order.DateCreated = currentDateTime;
                order.UserBoughtId = user.Id.ToString();

                string? orderId = await orderService.InsertAsync(order);
                if (orderId is not null)
                    return RedirectToAction("OrderConfirmation", "Home", new { newId = orderId });
                else
                    throw new Exception($"Inserting order failed for user {user.Id}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] BuyBooks :: An error occured while saving new order.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<ActionResult> OrderConfirmation(string newId)
        {
            try
            {
                OrderModel model = await orderService.GetOneByIdWithBooksAsync(newId);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] OrderConfirmation :: An error occured while fetching order with id {newId}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }
        }
    }
}
