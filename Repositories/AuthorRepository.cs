using Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repositories
{
    public interface IAuthorRepository : IBaseRepository<Author>
    {
        Task<List<Author>> GetAuthorsByPartialNameAsync(string partialName, int startIndex, int numberOfObjects, IClientSessionHandle? session = null);
        Task<long> GetNumberOfAuthorsByPartialNameAsync(string partialName, IClientSessionHandle? session = null);
        Task<Dictionary<string, string>> GetNamesOfAllAuthorsAsync(IClientSessionHandle? session = null);
    }

    public class AuthorRepository(IOptions<MongoSettings> mongoSettings, ILogger<AuthorRepository> logger) : BaseRepository<Author>(mongoSettings, logger), IAuthorRepository
    {
        public async Task<List<Author>> GetAuthorsByPartialNameAsync(string partialName, int startIndex, int numberOfObjects, IClientSessionHandle? session = null)
        {
            try
            {
                FilterDefinition<Author> filter;
                if (!String.IsNullOrWhiteSpace(partialName))
                {
                    var regexPattern = new BsonRegularExpression(partialName, "i");
                    filter = Builders<Author>.Filter.Regex("Name", regexPattern);
                }
                else
                {
                    filter = Builders<Author>.Filter.Empty;
                }

                return session is null ?
                    await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync() :
                    await _mongoCollection.Find(session,filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAuthorsByPartialNameAsync :: " +
                    $"An error occured while fetching authors whose name contains {partialName}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfAuthorsByPartialNameAsync(string partialName, IClientSessionHandle? session = null)
        {
            try
            {
                FilterDefinition<Author> filter;
                if (!String.IsNullOrWhiteSpace(partialName))
                {
                    var regexPattern = new BsonRegularExpression(partialName, "i");

                    filter = Builders<Author>.Filter.Regex("Name", regexPattern);
                }
                else
                {
                    filter = Builders<Author>.Filter.Empty;
                }

                return session is null ?
                    await _mongoCollection.CountDocumentsAsync(filter) :
                    await _mongoCollection.CountDocumentsAsync(session, filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfAuthorsByPartialNameAsync :: " +
                    $"An error occured while fetching total number of authors with partial name {partialName}.\n{ex}");
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetNamesOfAllAuthorsAsync(IClientSessionHandle? session = null)
        {
            try
            {
                var projection = Builders<Author>.Projection.Include(a => a.Id).Include(a => a.Name);
                var sort = Builders<Author>.Sort.Ascending(a => a.Name);
                var filter = Builders<Author>.Filter.Empty;

                List<Author> dataFromDb;

                dataFromDb = session is null ?
                    await _mongoCollection.Find(filter).Project<Author>(projection).Sort(sort).ToListAsync() :
                    await _mongoCollection.Find(session, filter).Project<Author>(projection).Sort(sort).ToListAsync();

                var dictionary = dataFromDb.ToDictionary(entity => entity.Id, entity => entity.Name);

                return dictionary;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNamesOfAllAuthorsAsync :: " +
                    $"An error occured while fetching names of all authors.\n{ex}");
                throw;
            }
        }
    }
}
