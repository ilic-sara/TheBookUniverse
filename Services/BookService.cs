using AutoMapper;
using Data;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Models.Books;

namespace Services
{
    public interface IBookService
    {
        Task<List<BookModel>> GetAllAsync();
        Task<List<BookModel>> GetMultipleByIdsAsync(List<string> ids);
        Task<List<BookModel>> PaginationSearchAsync(int startIndex = 0, int numberOfObjects = 24);
        Task<List<BookModel>> GetBooksByPartialTitleAsync(string partialTitle, int startIndex = 0, int numberOfObjects = 24);
        Task<List<BookModel>> GetFilteredBooksAsync(string partialTitle, List<string> genres, List<string> languages, int yearFrom, int yearTo, int priceFrom, int priceTo, int startIndex = 0, int numberOfObjects = 24);
        Task<List<BookModel>> GetBooksByOneGenreAsync(string genre, int numberOfBooks = 10);
        Task<BookModel> GetOneByIdAsync(string id);
        Task UpdateAsync(BookModel model);
        Task<string> InsertAsync(BookModel model);
        Task DeleteAsync(string id);
        Task<long> GetTotalNumberOfBooks();
        Task<long> GetTotalNumberOfFilteredBooks(string partialTitle, List<string> genres, List<string> languages, int yearFrom, int yearTo, int priceFrom, int priceTo);
        Task<long> GetTotalNumberOfBooksByPartialTitleAsync(string partialTitle);
    }

    public class BookService : IBookService
    {
        private readonly IBookRepository bookRepository;
        private readonly IAuthorRepository authorRepository;
        private readonly IMapper mapper;
        private readonly ILogger <BookService> logger;
        private readonly IMongoClient mongoClient;

        public BookService(IBookRepository bookReppository, 
                           IAuthorRepository authorRepository, 
                           IMapper mapper,
                           ILogger<BookService> logger,
                           IMongoClient mongoClient)
        {
            this.bookRepository = bookReppository;
            this.authorRepository = authorRepository;
            this.mapper = mapper;
            this.logger = logger;
            this.mongoClient = mongoClient;
        }

        public async Task<List<BookModel>> GetAllAsync()
        {
            try
            {
                var dataFromDb = await bookRepository.GetAllAsync();
                var listOfModels = mapper.Map<List<BookModel>>(dataFromDb);

                return listOfModels;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all books.\n{ex}");
                throw;
            }
        }

        public async Task<List<BookModel>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                var dataFromDb = await bookRepository.GetMultipleByIdsAsync(ids);
                var model = mapper.Map<List<BookModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: " +
                    $"An error occured while fetching books with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<List<BookModel>> PaginationSearchAsync(int startIndex = 0, int numberOfObjects = 24)
        {
            try
            {
                var dataFromDb = await bookRepository.PaginationSearchAsync(startIndex, numberOfObjects);
                var listOfModel = mapper.Map<List<BookModel>>(dataFromDb);

                return listOfModel;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] PaginationSearchAsync :: An error occured while fetching books.\n{ex}");
                throw;
            }
        }

        public async Task<BookModel> GetOneByIdAsync(string id)
        {
            try
            {
                var dataFromDb = await bookRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"Book with id {id} not found in database - dataFromDb is null.");

                var model = mapper.Map<BookModel>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching a book with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<BookModel>> GetBooksByPartialTitleAsync(string partialTitle, int startIndex = 0, int numberOfObjects = 24)
        {
            try
            {
                var dataFromDb = await bookRepository.GetBooksByPartialTitleAsync(partialTitle, startIndex, numberOfObjects);
                var model = mapper.Map<List<BookModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetBooksByPartialTitleAsync :: " +
                    $"An error occured while fetching books with partial title {partialTitle}.\n{ex}");
                throw;
            }
        }

        public async Task<List<BookModel>> GetBooksByOneGenreAsync(string genre, int numberOfBooks = 10)
        {
            try
            {
                var dataFromDb = await bookRepository.GetBooksByOneGenreAsync(genre, numberOfBooks);
                var model = mapper.Map<List<BookModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetBooksByOneGenreAsync :: " +
                    $"An error occured while fetching {numberOfBooks} books with genre {genre}.\n{ex}");
                throw;
            }
        }

        public async Task<List<BookModel>> GetFilteredBooksAsync(string partialTitle,
                                                List<string> genres,
                                                List<string> languages,
                                                int yearFrom, 
                                                int yearTo, 
                                                int priceFrom, 
                                                int priceTo,
                                                int startIndex = 0,
                                                int numberOfObjects = 24)
        {
            try
            {
                var dataFromDb = await bookRepository.GetFilteredBooksAsync(partialTitle, 
                                                                            genres, 
                                                                            languages, 
                                                                            yearFrom, 
                                                                            yearTo, 
                                                                            priceFrom, 
                                                                            priceTo, 
                                                                            startIndex, 
                                                                            numberOfObjects);
                var model = mapper.Map<List<BookModel>>(dataFromDb);

                logger.LogInformation($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[I N F O] GetFilteredBooksAsync :: Fetching filtered books:\n" +
                    $"Genres: {string.Join(", ", genres)}\n" +
                    $"Languages: {string.Join(", ", languages)}\n" +
                    $"From year {yearFrom} to year {yearTo}\n" +
                    $"From price {priceFrom} to price {priceTo}\n");

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetFilteredBooksAsync :: An error occured while fetching filtered books:\n" +
                    $"Genres: {string.Join(", ", genres)}\n" +
                    $"Languages: {string.Join(", ", languages)}\n" +
                    $"From year {yearFrom} to year {yearTo}\n" +
                    $"From price {priceFrom} to price {priceTo}\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(BookModel model)
        {
            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var bookEntity = mapper.Map<Book>(model);
                    string? newId = await bookRepository.InsertAsync(bookEntity, session);
                    if (!String.IsNullOrEmpty(newId))
                    {
                        var author = await authorRepository.GetOneByIdAsync(model.AuthorId, session);
                        if (author is null)
                            throw new Exception($"Author with ID {model.AuthorId} not found.");

                        if (author.BooksIds is null)
                            author.BooksIds = new();
                        author.BooksIds.Add(newId);
                        await authorRepository.UpdateAsync(author, session);

                        await session.CommitTransactionAsync();
                    }
                    return newId;
                }
                catch (Exception ex)
                {
                    logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] InsertAsync :: An error occured while inserting a book {model.Title}.\n{ex}");
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        public async Task UpdateAsync(BookModel updatedBook)
        {
            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var oldBook = await bookRepository.GetOneByIdAsync(updatedBook.Id, session);
                    if (oldBook is null)
                        return;
                    if (oldBook.AuthorId != updatedBook.AuthorId)
                    {
                        var newAuthor = await authorRepository.GetOneByIdAsync(updatedBook.AuthorId, session);
                        if (newAuthor is null)
                            return;
                        newAuthor.BooksIds ??= new();
                        newAuthor.BooksIds.Add(updatedBook.Id);
                        await authorRepository.UpdateAsync(newAuthor, session);

                        var oldAuthor = await authorRepository.GetOneByIdAsync(oldBook.AuthorId, session);
                        if (oldAuthor is not null && oldAuthor.BooksIds is not null)
                        {
                            if (oldAuthor.BooksIds.Contains(oldBook.Id))
                                oldAuthor.BooksIds.Remove(oldBook.Id);
                            await authorRepository.UpdateAsync(oldAuthor, session);
                        }
                    }
                    await bookRepository.UpdateAsync(mapper.Map<Book>(updatedBook), session);

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] UpdateAsync :: An error occured while updating book with id {updatedBook.Id}.\n{ex}");
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        public async Task DeleteAsync(string id)
        {
            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    Book book = await bookRepository.GetOneByIdAsync(id, session) ??
                        throw new Exception($"Book with id {id} not found.");

                    Author author = await authorRepository.GetOneByIdAsync(book.AuthorId, session) ??
                        throw new Exception($"Author with id {book.AuthorId} not found.");

                    author.BooksIds.Remove(book.Id);
                    await authorRepository.UpdateAsync(author, session);
                    await bookRepository.DeleteAsync(id, session);
                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] DeleteAsync :: An error occured while deleting book with id {id}.\n{ex}");
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        public async Task<long> GetTotalNumberOfBooks()
        {
            try
            {
                return await bookRepository.GetNumberOfDocumentsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetTotalNumberOfBooks :: An error occured while fetching number of all authors.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetTotalNumberOfBooksByPartialTitleAsync(string partialTitle)
        {
            return await bookRepository.GetNumberOfBooksByPartialTitleAsync(partialTitle);
        }

        public async Task<long> GetTotalNumberOfFilteredBooks(string partialTitle, 
                                               List<string> genres,
                                               List<string> languages,
                                               int yearFrom,
                                               int yearTo,
                                               int priceFrom,
                                               int priceTo)
        {
            try
            {
                return await bookRepository.GetNumberOfFilteredDocumentsAsync(partialTitle, genres, languages, yearFrom, yearTo, priceFrom, priceTo);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetTotalNumberOfFilteredBooks :: An error occured while fetching the number of filtered books:\n" +
                    $"Genres: {string.Join(", ", genres)}\n" +
                    $"Languages: {string.Join(", ", languages)}\n" +
                    $"From year {yearFrom} to year {yearTo}\n" +
                    $"From price {priceFrom} to price {priceTo}\n{ex}");
                throw;
            }
        }
    }
}
