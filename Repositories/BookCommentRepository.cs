using Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Repositories
{
    public interface IBookCommentRepository : IBaseRepository<BookComment>
    {

    }

    public class BookCommentRepository(IOptions<MongoSettings> mongoSettings, ILogger<BookCommentRepository> logger) 
        : BaseRepository<BookComment>(mongoSettings, logger), IBookCommentRepository
    {

    }
}
