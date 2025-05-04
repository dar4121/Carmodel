using CarmodelAPI.Models;
using CarmodelAPI.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarmodelAPI.Repository.Interface
{
    public interface ICarModelRepository
    {
        // Car Model operations
        Task<IEnumerable<CarModelViewModel>> GetAllCarModelsAsync(int skip = 0, int take = 10);
        Task<CarModelViewModel> GetCarModelByIdAsync(int modelId);
        Task<int> InsertUpdateCarModelAsync(CarModelCreateUpdateViewModel model);
        Task<bool> DeleteCarModelAsync(int modelId);
        Task<bool> UpdateSortOrderAsync(List<SortOrderUpdateViewModel> sortOrders);

        // Image operations
        Task<IEnumerable<ImageViewModel>> GetCarModelImagesAsync(int modelId);
        Task<int> UploadCarModelImageAsync(int modelId, IFormFile image, bool isDefault);
        Task<List<int>> UploadCarModelImagesAsync(int modelId, List<IFormFile> images, bool setFirstAsDefault);
        Task<bool> DeleteCarModelImageAsync(int imageId);
        Task<bool> SetDefaultImageAsync(int imageId, int modelId);

        // Brand and Class operations
        Task<IEnumerable<TblBrand>> GetAllBrandsAsync();
        Task<IEnumerable<TblClass>> GetAllClassesAsync();
    }
}