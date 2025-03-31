using Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repositories
{
    public interface IBookRepository : IBaseRepository<Book>
    {
        Task<Book> GetOneByNameAsync(string title, IClientSessionHandle? session = null);
        Task<List<Book>> GetMultipleByNameAsync(string title, int startIndex, int numberOfObjects, IClientSessionHandle? session = null);
        Task<List<Book>> GetBooksByPartialTitleAsync(string partialTitle, int startIndex, int numberOfObjects, IClientSessionHandle? session = null);
        Task<List<Book>> GetBooksByGenresAsync(List<string> genres, IClientSessionHandle? session = null);
        Task<List<Book>> GetBooksByOneGenreAsync(string genre, int numberOfBooks, IClientSessionHandle? session = null);
        Task<List<Book>> GetFilteredBooksAsync(string partialTitle, List<string> genres, List<string> languages, int yearFrom, 
                            int yearTo, int priceFrom, int priceTo, int startIndex, int numberOfObjects, IClientSessionHandle? session = null);
        Task<long> GetNumberOfFilteredDocumentsAsync(string partialTitle, List<string> genres, List<string> languages, 
                            int yearFrom, int yearTo, int priceFrom, int priceTo, IClientSessionHandle? session = null);
        Task<long> GetNumberOfBooksByPartialTitleAsync(string partialTitle, IClientSessionHandle? session = null);

    }

    public class BookRepository(IOptions<MongoSettings> mongoSettings, ILogger<BookRepository> logger)
        : BaseRepository<Book>(mongoSettings, logger), IBookRepository
    {
        public async Task<Book> GetOneByNameAsync(string title, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<Book>.Filter.Eq(x => x.Title, title);

                return session is null ?
                    await _mongoCollection.Find(filter).FirstOrDefaultAsync() :
                    await _mongoCollection.Find(session,filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByNameAsync :: An error occured while fetching book with the title {title}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Book>> GetBooksByPartialTitleAsync(string partialTitle, int startIndex, int numberOfObjects, IClientSessionHandle? session = null)
        {
            try
            {
                var regexPattern = new BsonRegularExpression(partialTitle, "i");

                var filter = Builders<Book>.Filter.Regex(x => x.Title, regexPattern);

                return session is null ?
                    await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync() :
                    await _mongoCollection.Find(session, filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetBooksByPartialTitleAsync :: " +
                    $"An error occured while fetching book with partial title {partialTitle}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Book>> GetMultipleByNameAsync(string title, int startIndex, int numberOfObjects, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<Book>.Filter.Eq(x => x.Title, title);

                return session is null ?
                    await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync() :
                    await _mongoCollection.Find(session, filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByNameAsync :: " +
                    $"An error occured while fetching books with name {title}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Book>> GetBooksByGenresAsync(List<string> genres, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<Book>.Filter.AnyIn(x => x.Genres, genres);

                return session is null ?
                    await _mongoCollection.Find(filter).ToListAsync() :
                    await _mongoCollection.Find(session, filter).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetBooksByGenresAsync :: " +
                    $"An error occured while fetching books with genres {string.Join(", ", genres)}.\n{ex}");
                throw;
            }

        }

        public async Task<List<Book>> GetBooksByOneGenreAsync(string genre, int numberOfBooks, IClientSessionHandle? session = null)
        {
            try
            {
                var filter = Builders<Book>.Filter.AnyEq(x => x.Genres, genre);

                return session is null ?
                    await _mongoCollection.Find(filter).Limit(numberOfBooks).ToListAsync() :
                    await _mongoCollection.Find(session, filter).Limit(numberOfBooks).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetBooksByOneGenreAsync :: " +
                    $"An error occured while fetching {numberOfBooks} books with the genre {genre}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Book>> GetFilteredBooksAsync(string partialTitle, 
                                                           List<string> genres,
                                                           List<string> languages,
                                                           int yearFrom,
                                                           int yearTo,
                                                           int priceFrom,
                                                           int priceTo,
                                                           int startIndex,
                                                           int numberOfObjects,
                                                           IClientSessionHandle? session = null)
        {
            try
            {
                var filterBuilder = Builders<Book>.Filter;
                var filters = new List<FilterDefinition<Book>>();
                
                if (!String.IsNullOrWhiteSpace(partialTitle))
                {
                    var regexPattern = new BsonRegularExpression(partialTitle, "i");
                    var partialTitleFilter = filterBuilder.Regex(book => book.Title, regexPattern);
                    filters.Add(partialTitleFilter);
                }

                if (genres.Any())
                {
                    var genresFilter = filterBuilder.AnyIn(book => book.Genres, genres);
                    filters.Add(genresFilter);
                }

                if (languages.Any())
                {
                    var languageFilter = filterBuilder.In(book => book.Language, languages);
                    filters.Add(languageFilter);
                }

                var yearFilter = filterBuilder.And(
                    filterBuilder.Gte(book => book.PublishedYear, yearFrom),
                    filterBuilder.Lte(book => book.PublishedYear, yearTo));
                filters.Add(yearFilter);

                var priceFilter = filterBuilder.And(
                    filterBuilder.Gte(book => book.Price, Convert.ToDecimal(priceFrom)),
                    filterBuilder.Lte(book => book.Price, Convert.ToDecimal(priceTo)));
                filters.Add(priceFilter);

                var combinedFilter = filterBuilder.And(filters);

                return session is null ?
                    await _mongoCollection.Find(combinedFilter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync() :
                    await _mongoCollection.Find(session, combinedFilter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetFilteredBooksAsync :: " +
                    $"An error occured while fetching filtered books.\n" +
                    $"Genres: {string.Join(", ", genres)}\n" + 
                    $"Languages: {string.Join(", ", languages)}\n" +
                    $"From year {yearFrom} to year {yearTo}\n" +
                    $"Price from {priceFrom} to price {priceTo}\n" +
                    $"{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfBooksByPartialTitleAsync(string partialTitle, IClientSessionHandle? session = null)
        {
            try
            {
                var regexPattern = new BsonRegularExpression(partialTitle, "i");

                var filter = Builders<Book>.Filter.Regex(x => x.Title, regexPattern);

                return session is null ?
                    await _mongoCollection.CountDocumentsAsync(filter) :
                    await _mongoCollection.CountDocumentsAsync(session, filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfBooksByPartialTitleAsync :: " +
                    $"An error occured while fetching total number of books with partial title {partialTitle}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfFilteredDocumentsAsync(string partialTitle, 
                                                                  List<string> genres,
                                                                  List<string> languages,
                                                                  int yearFrom,
                                                                  int yearTo,
                                                                  int priceFrom,
                                                                  int priceTo,
                                                                  IClientSessionHandle? session = null)
        {
            try
            {
                var filterBuilder = Builders<Book>.Filter;
                var filters = new List<FilterDefinition<Book>>();

                if (!String.IsNullOrWhiteSpace(partialTitle))
                {
                    var regexPattern = new BsonRegularExpression(partialTitle, "i");
                    var partialTitleFilter = Builders<Book>.Filter.Regex("Title", regexPattern);
                    filters.Add(partialTitleFilter);
                }

                if (genres.Any())
                {
                    var genresFilter = filterBuilder.AnyIn(book => book.Genres, genres);
                    filters.Add(genresFilter);
                }

                if (languages.Any())
                {
                    var languageFilter = filterBuilder.In(book => book.Language, languages);
                    filters.Add(languageFilter);
                }

                var yearFilter = filterBuilder.And(
                    filterBuilder.Gte(book => book.PublishedYear, yearFrom),
                    filterBuilder.Lte(book => book.PublishedYear, yearTo));
                filters.Add(yearFilter);

                var priceFilter = filterBuilder.And(
                    filterBuilder.Gte(book => book.Price, Convert.ToDecimal(priceFrom)),
                    filterBuilder.Lte(book => book.Price, Convert.ToDecimal(priceTo)));
                filters.Add(priceFilter);

                var combinedFilter = filterBuilder.And(filters);

                return session is null ?
                    await _mongoCollection.CountDocumentsAsync(combinedFilter) :
                    await _mongoCollection.CountDocumentsAsync(session, combinedFilter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfFilteredDocumentsAsync :: " +
                    $"An error occured while fetching number of filtered books.\n" +
                    $"Genres: {string.Join(", ", genres)}\n" +
                    $"Languages: {string.Join(", ", languages)}\n" +
                    $"From year {yearFrom} to year {yearTo}\n" +
                    $"Price from {priceFrom} to price {priceTo}\n" +
                    $"{ex}");
                throw;
            }
        }
    }
}
