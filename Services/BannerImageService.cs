using AutoMapper;
using Data;
using Repositories;
using Microsoft.Extensions.Logging;
using Models.Admin;

namespace Services
{
    public interface IBannerImageService
    {
        Task<List<BannerImageModel>> GetAllAsync();
        Task<List<BannerImageModel>> GetMultipleByIdsAsync(List<string> ids);
        Task<BannerImageModel> GetOneByIdAsync(string id);
        Task UpdateAsync(BannerImageModel model);
        Task<string> InsertAsync(BannerImageModel model);
        Task DeleteAsync(string id);
    }

    public class BannerImageService : IBannerImageService
    {
        private readonly IBannerImageRepository bannerImageRepository;
        private readonly IMapper mapper;
        private readonly ILogger<BannerImageService> logger;

        public BannerImageService(IBannerImageRepository bannerImageReppository, 
                                  IMapper mapper, 
                                  ILogger<BannerImageService> logger)
        {
            this.bannerImageRepository = bannerImageReppository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<List<BannerImageModel>> GetAllAsync()
        {
            try
            {
                var dataFromDb = await bannerImageRepository.GetAllAsync();
                var listOfModels = mapper.Map<List<BannerImageModel>>(dataFromDb);

                return listOfModels;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all banner images.\n{ex}");
                throw;
            }
        }

        public async Task<List<BannerImageModel>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                var dataFromDb = await bannerImageRepository.GetMultipleByIdsAsync(ids);
                var model = mapper.Map<List<BannerImageModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: " +
                    $"An error occured while fetching banner images with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<BannerImageModel> GetOneByIdAsync(string id)
        {
            try
            {
                var dataFromDb = await bannerImageRepository.GetOneByIdAsync(id) ?? 
                    throw new Exception($"Banner image with id {id} not found in database - dataFromDb is null.");

                var model = mapper.Map<BannerImageModel>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching banner image with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(BannerImageModel model)
        {
            try
            {
                return await bannerImageRepository.InsertAsync(mapper.Map<BannerImage>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting banner image.\n{ex}");
                throw;
            }
        }

        public async Task UpdateAsync(BannerImageModel model)
        {
            try
            {
                await bannerImageRepository.UpdateAsync(mapper.Map<BannerImage>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating banner image with id {model.Id}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await bannerImageRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: An error occured while deleting banner image with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
