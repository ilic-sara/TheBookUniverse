using AutoMapper;
using Data;
using MongoDB.Bson;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Models.Users;
using Models.Books;
using Models.Orders;

namespace Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllAsync();
        Task<UserModel> GetOneByIdAsync(ObjectId id);
        Task<User> GetOneUserByIdAsync(ObjectId id);
        Task<List<CartItemModel>> GetCartItemModelsForUser(ObjectId userId);
        Task<decimal> GetSumOfItemsInCartForUser(ObjectId userId);
        Task UpdateAsync(UserModel model);
        Task UpdateAsync(User model);
        Task<ObjectId> InsertAsync(UserModel model);
        Task DeleteAsync(string id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IBookRepository bookRepository;
        private readonly ILogger<UserService> logger;
        private readonly IMapper mapper;

        public UserService(IUserRepository userRepository,
                           IBookRepository bookRepository,
                           IMapper mapper,
                           ILogger<UserService> logger)
        {
            this.userRepository = userRepository;
            this.bookRepository = bookRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<List<UserModel>> GetAllAsync()
        {
            try
            {
                var dataFromDb = await userRepository.GetAllAsync();
                var listOfModel = mapper.Map<List<UserModel>>(dataFromDb);

                return listOfModel;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all users.\n{ex}");
                throw;
            }
        }

        public async Task<UserModel> GetOneByIdAsync(ObjectId id)
        {
            try
            {
                var dataFromDb = await userRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"User with id {id} not found in database - dataFromDb is null.");

                var model = mapper.Map<UserModel>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching user with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<User> GetOneUserByIdAsync(ObjectId id)
        {
            try
            {
                var dataFromDb = await userRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"User with id {id} not found in database - dataFromDb is null.");

                return dataFromDb;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneUserByIdAsync :: An error occured while fetching user with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<CartItemModel>> GetCartItemModelsForUser(ObjectId userId)
        {
            try
            {
                List<CartItem> dataFromDb = await userRepository.GetCartContentByUserIdAsync(userId);

                return await MapCartItemsToCartItemModels(dataFromDb);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetCartItemModelsForUser :: An error occured while fetching cart content for user with id {userId}.\n{ex}");
                throw;
            }
        }

        public async Task<decimal> GetSumOfItemsInCartForUser(ObjectId userId)
        {
            try
            {
                List<CartItemModel> items = await GetCartItemModelsForUser(userId);
                decimal sum = 0;
                foreach (var item in items)
                    sum += item.Book.Price * item.Quantity;
                return sum;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetSumOfItemsInCartForUser :: An error occured while getting sum of items in cart for user with id {userId}.\n{ex}");
                throw;
            }
        }

        public async Task<List<CartItemModel>> MapCartItemsToCartItemModels(List<CartItem> cartItems)
        {
            try
            {
                if (cartItems is null || cartItems.Count == 0)
                    return [];

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
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] MapCartItemsToCartItemModels :: An error occured while mapping cart items to models.\n{ex}");
                throw;
            }
        }

        public async Task UpdateAsync(UserModel model)
        {
            try
            {
                await userRepository.UpdateAsync(mapper.Map<User>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating user with id {model.Id}.\n{ex}");
                throw;
            }
        }

        public async Task UpdateAsync(User model)
        {
            try
            {
                await userRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating user with id {model.Id}.\n{ex}");
                throw;
            }
        }

        public async Task<ObjectId> InsertAsync(UserModel model)
        {
            try
            {
                return await userRepository.InsertAsync(mapper.Map<User>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting user with username {model.UserName}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await userRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: An error occured while deleting user with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
