using Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Repositories
{
    public interface IFilterOptionsRepository : IBaseRepository<FilterOptions>
    {
        Task<FilterOptions> GetValuesByNameAsync(string name, IClientSessionHandle? session = null);
    }

    public class FilterOptionsRepository(IOptions<MongoSettings> mongoSettings, ILogger<FilterOptionsRepository> logger)
        : BaseRepository<FilterOptions>(mongoSettings, logger), IFilterOptionsRepository
    {
        public async Task<FilterOptions> GetValuesByNameAsync(string name, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<FilterOptions>.Filter.Eq(x => x.Name, name);

                return session is null ?
                    await _mongoCollection.Find(filter).FirstOrDefaultAsync() :
                    await _mongoCollection.Find(session, filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetValuesByNameAsync :: " +
                    $"An error occured while fetching filter option with name {name}.\n{ex}");
                throw;
            }
        }
    }
}
