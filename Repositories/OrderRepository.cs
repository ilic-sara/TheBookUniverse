using Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Repositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<List<Order>> GetMultipleBySentValueAsync(bool sent, IClientSessionHandle? session = null);
    }

    public class OrderRepository(IOptions<MongoSettings> mongoSettings, ILogger<OrderRepository> logger) 
        : BaseRepository<Order>(mongoSettings, logger), IOrderRepository
    {
        public async Task<List<Order>> GetMultipleBySentValueAsync(bool isSent, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<Order>.Filter.Eq(x => x.Sent, isSent);

                return session is null ?
                    await _mongoCollection.Find(filter).ToListAsync() :
                    await _mongoCollection.Find(session, filter).ToListAsync();
            }
            catch (Exception ex)
            {
                string sent = isSent ? "sent" : "not sent";
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleBySentValueAsync :: " +
                    $"An error occured while fetching orders that were {sent}.\n{ex}");
                throw;
            }
        }
    }
}
