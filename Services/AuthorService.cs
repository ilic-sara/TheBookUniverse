using AutoMapper;
using Data;
using Repositories;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Models.Authors;
using Models.Books;

namespace Services
{
    public interface IAuthorService
    {
        Task<List<AuthorModel>> GetAllAsync();
        Task<List<AuthorModel>> PaginationSearchAsync(int startIndex = 0, int numberOfObjects = 24);
        Task<List<AuthorModel>> GetMultipleByIdsAsync(List<string> listOfIds);
        Task<AuthorModel> GetOneByIdAsync(string id);
        Task<AuthorModel> GetOneByIdWithBooksAsync(string id);
        Task<List<AuthorModel>> GetAuthorsByPartialNameAsync(string partialName, int startIndex = 0, int numberOfObjects = 24);
        Task<Dictionary<string, string>> GetNamesOfAllAuthorsAsync();
        Task UpdateAsync(AuthorModel model);
        Task<string> InsertAsync(AuthorModel model);
        Task DeleteAsync(string id);
        Task<long> GetTotalNumberOfAuthors();
        Task<long> GetNumberOfAuthorsByPartialName(string partialName);
    }

    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository authorRepository;
        private readonly IBookRepository bookRepository;
        private readonly IMongoClient mongoClient;
        private readonly IMapper mapper;
        private readonly ILogger<AuthorService> logger;


        public AuthorService(IAuthorRepository authorRepository,  IMapper mapper, IMongoClient mongoClient, IBookRepository bookRepository, ILogger<AuthorService> logger)
        {
            this.authorRepository = authorRepository;
            this.mapper = mapper;
            this.mongoClient = mongoClient;
            this.bookRepository = bookRepository;
            this.logger = logger;
        }

        public async Task<List<AuthorModel>> GetAllAsync()
        {
            try
            {
                List<Author> dataFromDb = await authorRepository.GetAllAsync();
                List<AuthorModel> listOfModel = mapper.Map<List<AuthorModel>>(dataFromDb);

                return listOfModel;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all authors.\n{ex}");
                throw;
            }
        }

        public async Task<List<AuthorModel>> GetAuthorsByPartialNameAsync(string partialName, int startIndex = 0, int numberOfObjects = 24)
        {
            try
            {
                List<Author> dataFromDb = await authorRepository.GetAuthorsByPartialNameAsync(partialName, startIndex, numberOfObjects);
                List<AuthorModel> model = mapper.Map<List<AuthorModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAuthorsByPartialNameAsync :: " +
                    $"An error occured while fetching authors with partial name {partialName}.\n{ex}");
                throw;
            }
        }

        public async Task<AuthorModel> GetOneByIdAsync(string id)
        {
            try
            {
                Author dataFromDb = await authorRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"Author with id {id} not found in database - dataFromDb is null.");

                AuthorModel model = mapper.Map<AuthorModel>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching author with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<AuthorModel>> PaginationSearchAsync(int startIndex = 0, int numberOfObjects = 24)
        {
            try
            {
                List<Author> dataFromDb = await authorRepository.PaginationSearchAsync(startIndex, numberOfObjects);
                List<AuthorModel> listOfModel = mapper.Map<List<AuthorModel>>(dataFromDb);

                return listOfModel;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] PaginationSearchAsync :: An error occured while fetching authors.\n{ex}");
                throw;
            }
        }

        public async Task<AuthorModel> GetOneByIdWithBooksAsync(string id)
        {
            try
            {
                Author dataFromDb = await authorRepository.GetOneByIdAsync(id) ??
                    throw new Exception($"Author with id {id} not found in database - dataFromDb is null.");

                List<Book> books = new();
                if (dataFromDb.BooksIds is not null && dataFromDb.BooksIds.Count >= 0)
                    books = await bookRepository.GetMultipleByIdsAsync(dataFromDb.BooksIds);
                AuthorModel model = mapper.Map<AuthorModel>(dataFromDb);
                model.Books = mapper.Map<List<BookModel>>(books);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdWithBooksAsync :: " +
                    $"An error occured while fetching author with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task UpdateAsync(AuthorModel model)
        {
            try
            {
                await authorRepository.UpdateAsync(mapper.Map<Author>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: " +
                    $"An error occured while updating author with id {model.Id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(AuthorModel model)
        {
            try
            {
                return await authorRepository.InsertAsync(mapper.Map<Author>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: " +
                    $"An error occured while inserting author with name {model.Name}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    Author author = await authorRepository.GetOneByIdAsync(id, session) 
                        ?? throw new Exception($"Author with id {id} not found.");

                    if(author.BooksIds is not null && author.BooksIds.Count > 0)
                        await bookRepository.DeleteManyAsync(author.BooksIds, session);

                    await authorRepository.DeleteAsync(id, session);

                    await session.CommitTransactionAsync();

                }
                catch (Exception ex)
                {
                    logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                         $"[ERROR] DeleteAsync :: " +
                         $"An error occured while deleting authors with id {id}.\n{ex}");
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        public async Task<List<AuthorModel>> GetMultipleByIdsAsync(List<string> listOfIds)
        {
            try
            {
                List<Author> dataFromDb = await authorRepository.GetMultipleByIdsAsync(listOfIds);
                List<AuthorModel> model = mapper.Map<List<AuthorModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: " +
                    $"An error occured while fetching authors with ids {string.Join(", ", listOfIds)}.\n{ex}");
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetNamesOfAllAuthorsAsync()
        {
            try
            {
                Dictionary<string, string> dataFromDb = 
                    await authorRepository.GetNamesOfAllAuthorsAsync() ??
                    throw new Exception("Failed to retrieve names of all authors - dataFromDb is null.");

                return dataFromDb;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNamesOfAllAuthorsAsync :: " +
                    $"An error occured while fetching names of all authors.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetTotalNumberOfAuthors()
        {
            try
            {
                return await authorRepository.GetNumberOfDocumentsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetTotalNumberOfAuthors :: " +
                    $"An error occured while fetching number of all authors.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfAuthorsByPartialName(string partialName)
        {
            try
            {
                return await authorRepository.GetNumberOfAuthorsByPartialNameAsync(partialName);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfAuthorsByPartialName :: " +
                    $"An error occured while fetching number of authors whose name contains {partialName}.\n{ex}");
                throw;
            }
        }
    }
}
