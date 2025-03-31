using Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Repositories
{
    public interface IBannerImageRepository : IBaseRepository<BannerImage>
    {

    }

    public class BannerImageRepository(IOptions<MongoSettings> mongoSettings, ILogger<BannerImageRepository> logger) 
        : BaseRepository<BannerImage>(mongoSettings, logger), IBannerImageRepository
    {

    }
}
