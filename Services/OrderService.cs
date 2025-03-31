using AutoMapper;
using Data;
using MongoDB.Bson;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Models.Books;
using Models.Orders;

namespace Services
{
    public interface IOrderService
    {
        Task<List<OrderModel>> GetAllAsync();
        Task<List<OrderModel>> GetMultipleByIdsAsync(List<string> ids);
        Task<OrderModel> GetOneByIdAsync(string id);
        Task<OrderModel> GetOneByIdWithBooksAsync(string id);
        Task<List<OrderModel>> GetMultipleBySentValueAsync(bool sent);
        Task<List<OrderModel>> GetOrdersByUserIdAsync(string userId);

        Task UpdateAsync(OrderModel model);
        Task MarkAsSentAsync(string id);
        Task<string> InsertAsync(OrderModel model);
        Task DeleteAsync(string id);

    }
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IBookRepository bookRepository;
        private readonly IMongoClient mongoClient;
        private readonly ILogger<OrderService> logger;
        private readonly IMapper mapper;

        public OrderService(IOrderRepository orderReppository, 
                            IUserRepository userRepository, 
                            IBookRepository bookRepository,
                            IMongoClient mongoClient,
                            IMapper mapper,
                            ILogger<OrderService> logger)
        {
            this.orderRepository = orderReppository;
            this.userRepository = userRepository;
            this.bookRepository = bookRepository;
            this.mongoClient = mongoClient;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<List<OrderModel>> GetAllAsync()
        {
            try
            {
                List<Order> dataFromDb = await orderRepository.GetAllAsync();
                List<OrderModel> models = mapper.Map<List<OrderModel>>(dataFromDb);
                foreach (var model in models)
                {
                    var orderItems = dataFromDb.FirstOrDefault(i => i.Id == model.Id)?.Items;
                    model.Items = await MapCartItemsToCartItemModels(orderItems);
                }

                return models;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all orders.\n{ex}");
                throw;
            }
        }

        public async Task<List<OrderModel>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                List<Order> dataFromDb = await orderRepository.GetMultipleByIdsAsync(ids);
                List<OrderModel> models = mapper.Map<List<OrderModel>>(dataFromDb);
                foreach (var model in models)
                {
                    var orderItems = dataFromDb.FirstOrDefault(i => i.Id == model.Id)?.Items;
                    model.Items = await MapCartItemsToCartItemModels(orderItems);
                }

                return models;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching orders with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<OrderModel> GetOneByIdAsync(string id)
        {
            try
            {
                Order dataFromDb = await orderRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"Order with id {id} not found in database - dataFromDb is null.");

                OrderModel model = mapper.Map<OrderModel>(dataFromDb);
                model.Items = await MapCartItemsToCartItemModels(dataFromDb.Items);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching order with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<OrderModel> GetOneByIdWithBooksAsync(string id)
        {
            try
            {
                Order dataFromDb = await orderRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"Order with id {id} not found in database - dataFromDb is null.");

                List<Book> books = await bookRepository.GetMultipleByIdsAsync(dataFromDb.Items.Select(i => i.BookId).ToList());
                List<BookModel> bookModels = mapper.Map<List<BookModel>>(books);
                OrderModel model = mapper.Map<OrderModel>(dataFromDb);
                model.Items = await MapCartItemsToCartItemModels(dataFromDb.Items);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdWithBooksAsync :: An error occured while fetching order with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<OrderModel>> GetMultipleBySentValueAsync(bool isSent)
        {
            try
            {
                List<Order> dataFromDb = await orderRepository.GetMultipleBySentValueAsync(isSent);
                List<OrderModel> models = mapper.Map<List<OrderModel>>(dataFromDb);
                foreach (var model in models)
                {
                    model.Items = await MapCartItemsToCartItemModels(dataFromDb.FirstOrDefault(i => i.Id == model.Id)?.Items);
                }
                return models;
            }
            catch (Exception ex)
            {
                string sent = isSent ? "sent" : "not sent";
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleBySentValueAsync :: An error occured while fetching orders that were {sent}.\n{ex}");
                throw;
            }
        }

        public async Task<List<OrderModel>> GetOrdersByUserIdAsync(string userId)
        {
            try
            {
                List<string> ordersIdsByUser = await userRepository.GetListOfOrdersIdsByUserIdAsync(userId);
                List<Order> ordersByUser = await orderRepository.GetMultipleByIdsAsync(ordersIdsByUser);
                List<OrderModel> models = mapper.Map<List<OrderModel>>(ordersByUser);
                foreach (var model in models)
                {
                    model.Items = await MapCartItemsToCartItemModels(ordersByUser.FirstOrDefault(i => i.Id == model.Id)?.Items);
                }
                return models;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOrdersByUserIdAsync :: An error occured while fetching orders for user with id {userId}.\n{ex}");
                throw;
            }
        }

        public async Task<List<CartItemModel>> MapCartItemsToCartItemModels(List<CartItem>? cartItems)
        {
            try
            {
                if(cartItems is null || cartItems.Count == 0)
                    return new();

                List<Book> books = await bookRepository.GetMultipleByIdsAsync(cartItems.Select(b => b.BookId).ToList());
                List<CartItemModel> model = new();

                foreach (var dataItem in cartItems)
                {
                    Book book = books.FirstOrDefault(b => b.Id == dataItem.BookId) ?? 
                        throw new Exception($"Book with id {dataItem.BookId} doesn't exist in database.");

                    model.Add(new CartItemModel { Book = mapper.Map<BookModel>(book), Quantity = dataItem.Quantity });
                }
                return model;
            }
            catch(Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] MapCartItemsToCartItemModels :: An error occured while mapping cart items to models.\n{ex}");
                throw;
            }
        }


        public async Task<string> InsertAsync(OrderModel model)
        {
            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    string? newId = await orderRepository.InsertAsync(mapper.Map<Order>(model), session);
                    if (!String.IsNullOrEmpty(newId))
                    {
                        //Update user's orders
                        User user = await userRepository.GetOneByIdAsync(ObjectId.Parse(model.UserBoughtId), session) ??
                            throw new Exception($"User with id {model.UserBoughtId} not found for order with id {newId}.");

                        if (user.OrdersIds is null)
                            user.OrdersIds = new();

                        user.OrdersIds.Add(newId);
                        user.CartItems = new();
                        await userRepository.UpdateAsync(user, session);

                        List<Book> books = await bookRepository.GetMultipleByIdsAsync(model.Items.Select(x => x.Book.Id).ToList(), session);
                        //Update book inventory
                        foreach (var book in books)
                        {
                            CartItemModel cartItemModel = model.Items.FirstOrDefault(a => a.Book.Id == book.Id) ?? new();
                            int amount = cartItemModel.Quantity;
                            if (book.Inventory < amount)
                                throw new Exception($"Not enough books: Attempting to buy {amount} books " +
                                    $"with id {book.Id} and title {book.Title}, but the inventory is {book.Inventory}.");
                            book.Inventory = book.Inventory - amount;
                            await bookRepository.UpdateAsync(mapper.Map<Book>(book), session);
                        }

                        await session.CommitTransactionAsync();
                    }
                    return newId;
                }
                catch (Exception ex)
                {
                    logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] InsertAsync :: An error occured while inserting order for user {model.UserBoughtId}.\n{ex}");
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        public async Task UpdateAsync(OrderModel model)
        {
            try
            {
                await orderRepository.UpdateAsync(mapper.Map<Order>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating order with id {model.Id}.\n{ex}");
                throw;
            }
        }

        public async Task MarkAsSentAsync(string id)
        {
            try
            {
                Order order = await orderRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"Order with id {id} not found in database - order is null.");

                order.Sent = true;
                await orderRepository.UpdateAsync(order);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] MarkAsSentAsync :: An error occured while updating order with id {id} to sent.\n{ex}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await orderRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: An error occured while deleting order with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
