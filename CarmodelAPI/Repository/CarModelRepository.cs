using CarmodelAPI.Models;
using CarmodelAPI.Models.ViewModels;
using CarmodelAPI.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarmodelAPI.Repository
{
    public class CarModelRepository : ICarModelRepository
    {
        private readonly CarModelContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _imageUploadPath;

        public CarModelRepository(CarModelContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _imageUploadPath = _configuration["ImageSettings:UploadPath"] ?? "wwwroot/images/cars";


            if (!Directory.Exists(_imageUploadPath))
            {
                Directory.CreateDirectory(_imageUploadPath);
            }
        }


        public async Task<IEnumerable<TblBrand>> GetAllBrandsAsync()
        {
            return await _dbContext.TblBrands
                .OrderBy(b => b.BrandName)
                .ToListAsync();
        }

        public async Task<IEnumerable<TblClass>> GetAllClassesAsync()
        {
            return await _dbContext.TblClasses
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarModelViewModel>> GetAllCarModelsAsync(int skip = 0, int take = 10)
        {

            var carModels = await _dbContext.TblCarModels
                .Where(m => !m.IsDelete.HasValue || !m.IsDelete.Value)
                .OrderBy(m => m.Sortorder)
                .ThenBy(m => m.ModelId)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var result = new List<CarModelViewModel>();

            foreach (var model in carModels)
            {

                var brand = await _dbContext.TblBrands.FirstOrDefaultAsync(b => b.BrandId == model.BrandId);
                var className = await _dbContext.TblClasses.FirstOrDefaultAsync(c => c.ClassId == model.ClassId);

                var carViewModel = new CarModelViewModel
                {
                    ModelId = model.ModelId,
                    ModelName = model.ModelName,
                    ModelCode = model.ModelCode,
                    BrandId = model.BrandId,
                    BrandName = brand?.BrandName,
                    ClassId = model.ClassId,
                    ClassName = className?.ClassName,
                    Description = model.Description,
                    Features = model.Features,
                    Price = model.Price,
                    DateofManufacturing = model.DateofManufacturing,
                    IsActive = model.IsActive,
                    SortOrder = model.Sortorder,
                    DefaultImageUrl = await GetDefaultImageUrlAsync(model.ModelId)
                };

                result.Add(carViewModel);
            }

            return result;
        }

        public async Task<CarModelViewModel> GetCarModelByIdAsync(int modelId)
        {
            var model = await _dbContext.TblCarModels
                .FirstOrDefaultAsync(m => m.ModelId == modelId);

            if (model == null)
                return new CarModelViewModel();


            var brand = await _dbContext.TblBrands.FirstOrDefaultAsync(b => b.BrandId == model.BrandId);
            var className = await _dbContext.TblClasses.FirstOrDefaultAsync(c => c.ClassId == model.ClassId);

            var carViewModel = new CarModelViewModel
            {
                ModelId = model.ModelId,
                ModelName = model.ModelName,
                ModelCode = model.ModelCode,
                BrandId = model.BrandId,
                BrandName = brand?.BrandName,
                ClassId = model.ClassId,
                ClassName = className?.ClassName,
                Description = model.Description,
                Features = model.Features,
                Price = model.Price,
                DateofManufacturing = model.DateofManufacturing,
                IsActive = model.IsActive,
                SortOrder = model.Sortorder,
                DefaultImageUrl = await GetDefaultImageUrlAsync(model.ModelId)
            };

            return carViewModel;
        }

        public async Task<int> InsertUpdateCarModelAsync(CarModelCreateUpdateViewModel model)
        {
            try
            {
                // Check if a model with the same code already exists (except the current one being updated)
                bool modelCodeExists = await _dbContext.TblCarModels
                    .AnyAsync(m => m.ModelCode == model.ModelCode
                                && (m.IsDelete == null || m.IsDelete == false)
                                && m.ModelId != (model.ModelId ?? 0));

                if (modelCodeExists)
                {
                    throw new InvalidOperationException($"A car model with code '{model.ModelCode}' already exists.");
                }

                if (model.ModelId.HasValue && model.ModelId > 0)
                {
                    // Update existing model
                    var existingModel = await _dbContext.TblCarModels
                        .FirstOrDefaultAsync(m => m.ModelId == model.ModelId);

                    if (existingModel == null)
                    {
                        return 0;
                    }

                    // Update model properties but keep the original sort order
                    existingModel.BrandId = model.BrandId;
                    existingModel.ClassId = model.ClassId;
                    existingModel.ModelName = model.ModelName;
                    existingModel.ModelCode = model.ModelCode;
                    existingModel.Description = model.Description;
                    existingModel.Features = model.Features;
                    existingModel.Price = model.Price;
                    existingModel.DateofManufacturing = model.DateofManufacturing;
                    // Note: We do not change the sort order during update

                    await _dbContext.SaveChangesAsync();

                    return existingModel.ModelId;
                }
                else
                {
                    // Create new model
                    int nextSortOrder = 1;

                    if (await _dbContext.TblCarModels.AnyAsync(m => m.IsDelete == false || m.IsDelete == null))
                    {
                        nextSortOrder = await _dbContext.TblCarModels
                            .Where(m => m.IsDelete == false || m.IsDelete == null)
                            .MaxAsync(m => (int?)m.Sortorder ?? 0) + 1;
                    }

                    var newModel = new TblCarModel
                    {
                        BrandId = model.BrandId,
                        ClassId = model.ClassId,
                        ModelName = model.ModelName,
                        ModelCode = model.ModelCode,
                        Description = model.Description,
                        Features = model.Features,
                        Price = model.Price,
                        DateofManufacturing = model.DateofManufacturing,
                        IsActive = true,
                        IsDelete = false,
                        Sortorder = nextSortOrder
                    };

                    _dbContext.TblCarModels.Add(newModel);
                    await _dbContext.SaveChangesAsync();

                    return newModel.ModelId;
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Validation error in InsertUpdateCarModelAsync: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertUpdateCarModelAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> DeleteCarModelAsync(int modelId)
        {
            try
            {
                var model = await _dbContext.TblCarModels.FirstOrDefaultAsync(m => m.ModelId == modelId);
                if (model == null)
                    return false;


                model.IsDelete = true;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSortOrderAsync(List<SortOrderUpdateViewModel> sortOrders)
        {
            try
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // Validate input
                    if (sortOrders == null || !sortOrders.Any())
                    {
                        return false;
                    }

                    // Get all car models that are not deleted
                    var allModels = await _dbContext.TblCarModels
                        .Where(m => !m.IsDelete.HasValue || !m.IsDelete.Value)
                        .ToListAsync();

                    // Create a dictionary for quick lookup of models by ID
                    var modelDict = allModels.ToDictionary(m => m.ModelId);

                    // Validate that we have all models in the update request
                    foreach (var item in sortOrders)
                    {
                        if (!modelDict.ContainsKey(item.ModelId))
                        {
                            // Model doesn't exist or is deleted
                            return false;
                        }
                    }

                    // Create a set of all model IDs included in the sort order update
                    var updatedModelIds = sortOrders.Select(s => s.ModelId).ToHashSet();

                    // Find any models that aren't included in the sort order update
                    var nonUpdatedModels = allModels.Where(m => !updatedModelIds.Contains(m.ModelId)).ToList();

                    // First update all models in the sort order request
                    foreach (var item in sortOrders)
                    {
                        if (modelDict.TryGetValue(item.ModelId, out var model))
                        {
                            model.Sortorder = item.SortOrder;
                        }
                    }

                    // Handle edge case: if we have models not included in the update
                    if (nonUpdatedModels.Any())
                    {
                        // Find the next available sort order
                        int nextSortOrder = sortOrders.Any() ? sortOrders.Max(s => s.SortOrder) + 1 : 1;

                        // Sort non-updated models by their current sort order to preserve relative ordering
                        nonUpdatedModels = nonUpdatedModels.OrderBy(m => m.Sortorder).ToList();

                        // Assign sequential sort orders
                        foreach (var model in nonUpdatedModels)
                        {
                            model.Sortorder = nextSortOrder++;
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error in UpdateSortOrderAsync: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateSortOrderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<ImageViewModel>> GetCarModelImagesAsync(int modelId)
        {
            var images = await _dbContext.TblImages
                .Where(i => i.ModelId == modelId)
                .Select(i => new ImageViewModel
                {
                    ImageId = i.ImageId,
                    ModelId = i.ModelId ?? 0,
                    ImageName = i.ImageName ?? string.Empty,
                    ImageUrl = $"/images/cars/{i.ImageName}",
                    IsDefault = i.IsDefault ?? false
                })
                .ToListAsync();

            return images;
        }

        public async Task<int> UploadCarModelImageAsync(int modelId, IFormFile image, bool isDefault)
        {
            try
            {

                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(_imageUploadPath, fileName);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }


                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {

                    if (isDefault)
                    {
                        var existingImages = await _dbContext.TblImages
                            .Where(i => i.ModelId == modelId)
                            .ToListAsync();

                        foreach (var img in existingImages)
                        {
                            img.IsDefault = false;
                        }

                        await _dbContext.SaveChangesAsync();
                    }


                    var imageEntity = new TblImage
                    {
                        ModelId = modelId,
                        ImageName = fileName,
                        IsDefault = isDefault
                    };

                    _dbContext.TblImages.Add(imageEntity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return imageEntity.ImageId;
                }
                catch
                {
                    await transaction.RollbackAsync();


                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<int>> UploadCarModelImagesAsync(int modelId, List<IFormFile> images, bool setFirstAsDefault)
        {
            List<int> uploadedImageIds = new List<int>();

            try
            {

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {

                    if (setFirstAsDefault && images.Count > 0)
                    {
                        var existingImages = await _dbContext.TblImages
                            .Where(i => i.ModelId == modelId)
                            .ToListAsync();

                        foreach (var img in existingImages)
                        {
                            img.IsDefault = false;
                        }

                        await _dbContext.SaveChangesAsync();
                    }


                    for (int i = 0; i < images.Count; i++)
                    {
                        var image = images[i];
                        bool isDefault = setFirstAsDefault && i == 0;


                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        string filePath = Path.Combine(_imageUploadPath, fileName);


                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }


                        var imageEntity = new TblImage
                        {
                            ModelId = modelId,
                            ImageName = fileName,
                            IsDefault = isDefault
                        };

                        _dbContext.TblImages.Add(imageEntity);
                        await _dbContext.SaveChangesAsync();
                        uploadedImageIds.Add(imageEntity.ImageId);
                    }

                    await transaction.CommitAsync();
                    return uploadedImageIds;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();


                    foreach (var img in uploadedImageIds)
                    {
                        var image = await _dbContext.TblImages.FindAsync(img);
                        if (image != null && !string.IsNullOrEmpty(image.ImageName))
                        {
                            string filePath = Path.Combine(_imageUploadPath, image.ImageName);
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }

                    Console.WriteLine($"Error in UploadCarModelImagesAsync: {ex.Message}");
                    return new List<int>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadCarModelImagesAsync: {ex.Message}");
                return new List<int>();
            }
        }

        public async Task<bool> DeleteCarModelImageAsync(int imageId)
        {
            try
            {
                var image = await _dbContext.TblImages.FirstOrDefaultAsync(i => i.ImageId == imageId);
                if (image == null)
                    return false;


                if (!string.IsNullOrEmpty(image.ImageName))
                {
                    string filePath = Path.Combine(_imageUploadPath, image.ImageName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }


                _dbContext.TblImages.Remove(image);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetDefaultImageAsync(int imageId, int modelId)
        {
            try
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {

                    var images = await _dbContext.TblImages
                        .Where(i => i.ModelId == modelId)
                        .ToListAsync();

                    foreach (var img in images)
                    {
                        img.IsDefault = (img.ImageId == imageId);
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        private async Task<string> GetDefaultImageUrlAsync(int modelId)
        {
            var defaultImage = await _dbContext.TblImages
                .FirstOrDefaultAsync(i => i.ModelId == modelId && i.IsDefault == true);

            if (defaultImage != null && !string.IsNullOrEmpty(defaultImage.ImageName))
            {
                return $"/images/cars/{defaultImage.ImageName}";
            }

            return "/images/cars/default-car.jpg";
        }
    }
}