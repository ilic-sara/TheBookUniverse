using Microsoft.Extensions.Logging;
using AutoMapper;
using Data;
using Repositories;
using Models.Admin;

namespace Services
{
    public interface IFilterOptionsService
    {
        Task<List<FilterOptionsModel>> GetAllAsync();
        Task<List<FilterOptionsModel>> GetMultipleByIdsAsync(List<string> ids);
        Task<FilterOptionsModel> GetOneByIdAsync(string id);
        Task<List<string>> GetValuesByNameAsync(string name);
        Task UpdateAsync(FilterOptionsModel model);
        Task<string> InsertAsync(FilterOptionsModel model);
        Task DeleteAsync(string id);
    }

    public class FilterOptionsService : IFilterOptionsService
    {
        private readonly IFilterOptionsRepository filterOptionsRepository;
        private readonly IMapper mapper;
        private readonly ILogger<FilterOptionsService> logger;

        public FilterOptionsService(IFilterOptionsRepository filterOptionsReppository, 
                                    IMapper mapper,
                                    ILogger<FilterOptionsService> logger)
        {
            this.filterOptionsRepository = filterOptionsReppository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<List<FilterOptionsModel>> GetAllAsync()
        {
            try
            {
                var dataFromDb = await filterOptionsRepository.GetAllAsync();
                var listOfModels = mapper.Map<List<FilterOptionsModel>>(dataFromDb);

                return listOfModels;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all filter options.\n{ex}");
                throw;
            }
        }

        public async Task<List<FilterOptionsModel>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                var dataFromDb = await filterOptionsRepository.GetMultipleByIdsAsync(ids);
                var model = mapper.Map<List<FilterOptionsModel>>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching filter options with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<FilterOptionsModel> GetOneByIdAsync(string id)
        {
            try
            {
                var dataFromDb = await filterOptionsRepository.GetOneByIdAsync(id) ?? 
                    throw new Exception($"Filter option with id {id} not found in database - dataFromDb is null.");

                var model = mapper.Map<FilterOptionsModel>(dataFromDb);

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching filter option with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<string>> GetValuesByNameAsync(string name)
        {
            try
            {
                var dataFromDb = await filterOptionsRepository.GetValuesByNameAsync(name) ??
                    throw new Exception($"Filter option with name {name} not found in database - dataFromDb is null.");

                var model = mapper.Map<FilterOptionsModel>(dataFromDb);
                if (model is null || model.Values is null)
                    return new();

                return model.Values;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetValuesByNameAsync :: An error occured while fetching filter options with name {name}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(FilterOptionsModel model)
        {
            try
            {
                return await filterOptionsRepository.InsertAsync(mapper.Map<FilterOptions>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting a filter option with a name {model.Name}.\n{ex}");
                throw;
            }
        }

        public async Task UpdateAsync(FilterOptionsModel model)
        {
            try
            {
                await filterOptionsRepository.UpdateAsync(mapper.Map<FilterOptions>(model));
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating filter option with id {model.Id}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await filterOptionsRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: An error occured while deleting filter option with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
