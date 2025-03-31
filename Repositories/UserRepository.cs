using Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;
using System.Reflection;

namespace Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync(IClientSessionHandle? session = null);
        Task<long> GetNumberOfDocumentsAsync(IClientSessionHandle? session = null);
        Task<User?> GetOneByIdAsync(ObjectId id, IClientSessionHandle? session = null);
        Task<List<CartItem>> GetCartContentByUserIdAsync(ObjectId id, IClientSessionHandle? session = null);
        Task<List<string>> GetListOfOrdersIdsByUserIdAsync(string id, IClientSessionHandle? session = null);
        Task<ObjectId> InsertAsync(User obj, IClientSessionHandle? session = null);
        Task DeleteAsync(string id, IClientSessionHandle? session = null);
        Task UpdateAsync(User obj, IClientSessionHandle? session = null);
    }

    public class UserRepository : IUserRepository
    {

        private readonly IMongoClient mongoClient;
        private readonly IMongoDatabase mongoDatabase;
        private readonly IMongoCollection<User> _mongoCollection;
        private readonly IConfiguration configuration;
        private readonly ILogger<UserRepository> logger;

        public UserRepository(IOptions<MongoSettings> mongoSettings, IConfiguration configuration, ILogger<UserRepository> logger)
        {
            this.configuration = configuration;
            mongoClient = new MongoClient(mongoSettings.Value.Connection);
            mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            var collectionNameAttribute = typeof(User).GetTypeInfo().GetCustomAttribute<CollectionNameAttribute>();
            var collectionName = collectionNameAttribute?.Name;
            _mongoCollection = mongoDatabase.GetCollection<User>(collectionName);
            this.logger = logger;
        }

        public async Task<List<User>> GetAllAsync(IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<User>.Filter.Empty;

                return session is null ?
                        await _mongoCollection.Find(filter).ToListAsync() :
                        await _mongoCollection.Find(session, filter).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: " +
                    $"An error occured while fetching all users.\n{ex}");
                throw;
            }
        }

        public async Task<List<CartItem>> GetCartContentByUserIdAsync(ObjectId id, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq("_id", id);

                User user = session is null ?
                    await _mongoCollection.Find(filter).FirstOrDefaultAsync() :
                    await _mongoCollection.Find(session, filter).FirstOrDefaultAsync();
                    
                return user.CartItems;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetCartContentByUserIdAsync :: " +
                    $"An error occured while fetching cart content for user with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<string>> GetListOfOrdersIdsByUserIdAsync(string id, IClientSessionHandle? session = null)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    throw new ArgumentException($"Invalid ID format, " +
                        $"couldn't parse ObjectId from provided string {id}.");
                }

                var filter = Builders<User>.Filter.Eq("_id", objectId);

                User user = session is null ?
                    await _mongoCollection.Find(filter).FirstOrDefaultAsync() :
                    await _mongoCollection.Find(session, filter).FirstOrDefaultAsync();

                List<string> ordersIdsByUser = new();
                if (user is not null)
                    ordersIdsByUser = user.OrdersIds;

                return ordersIdsByUser;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetListOfOrdersIdsByUserIdAsync :: " +
                    $"An error occured while fetching orders for user with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<User?> GetOneByIdAsync(ObjectId id, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq("_id", id);
                return session is null ?
                    await _mongoCollection.Find(filter).FirstOrDefaultAsync() :
                    await _mongoCollection.Find(session, filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: " +
                    $"An error occured while fetching user with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<ObjectId> InsertAsync(User obj, IClientSessionHandle? session = null)
        {
            try
            {
                await (session is null ?
                    _mongoCollection.InsertOneAsync(obj) :
                    _mongoCollection.InsertOneAsync(session, obj));

                return obj.Id;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: " +
                    $"An error occured while inserting new user with username {obj.UserName}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfDocumentsAsync(IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<User>.Filter.Empty;

                return session is null ?
                    await _mongoCollection.CountDocumentsAsync(filter) :
                    await _mongoCollection.CountDocumentsAsync(session, filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfDocumentsAsync :: " +
                    $"An error occured while fetching the number of documents in User collection.\n{ex}");
                throw;
            }
        }

        public async Task UpdateAsync(User obj, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq("_id", obj.Id);

                await (session is null ?
                    _mongoCollection.ReplaceOneAsync(filter, obj) :
                    _mongoCollection.ReplaceOneAsync(session, filter, obj));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: " +
                    $"An error occured while updating user with id {obj.Id}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteAsync(string id, IClientSessionHandle? session = null)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    throw new ArgumentException($"Invalid ID format, couldn't parse ObjectId from provided string {id}.");
                }

                var filter = Builders<User>.Filter.Eq("_id", objectId);

                await (session is null ?
                    _mongoCollection.DeleteOneAsync(filter) :
                    _mongoCollection.DeleteOneAsync(session, filter));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: " +
                    $"An error occured while deleting user with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
