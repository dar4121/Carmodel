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

            // Ensure directory exists
            if (!Directory.Exists(_imageUploadPath))
            {
                Directory.CreateDirectory(_imageUploadPath);
            }
        }

        // Brand and Class operations
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
            // Using EF Core instead of ADO.NET
            // Query car models with pagination using parameters instead of hardcoded values
            var carModels = await _dbContext.TblCarModels
                .Where(m => !m.IsDelete.HasValue || !m.IsDelete.Value)
                .OrderBy(m => m.Sortorder)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var result = new List<CarModelViewModel>();

            foreach (var model in carModels)
            {
                // Get the brand and class information since they're not directly in TblCarModel
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
                return new CarModelViewModel(); // Return empty view model instead of null

            // Get the brand and class information
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
                // Check if a car model with the same code already exists (excluding current model when updating)
                bool modelCodeExists = await _dbContext.TblCarModels
                    .AnyAsync(m => m.ModelCode == model.ModelCode
                                && (m.IsDelete == null || m.IsDelete == false)
                                && m.ModelId != (model.ModelId ?? 0));

                if (modelCodeExists)
                {
                    // Model code already exists
                    throw new InvalidOperationException($"A car model with code '{model.ModelCode}' already exists.");
                }

                if (model.ModelId.HasValue && model.ModelId > 0)
                {
                    // Update existing car model
                    var existingModel = await _dbContext.TblCarModels
                        .FirstOrDefaultAsync(m => m.ModelId == model.ModelId);

                    if (existingModel == null)
                    {
                        return 0;
                    }

                    // Update properties
                    existingModel.BrandId = model.BrandId;
                    existingModel.ClassId = model.ClassId;
                    existingModel.ModelName = model.ModelName;
                    existingModel.ModelCode = model.ModelCode;
                    existingModel.Description = model.Description;
                    existingModel.Features = model.Features;
                    existingModel.Price = model.Price;
                    existingModel.DateofManufacturing = model.DateofManufacturing;
                    // We don't update IsActive, IsDelete, and SortOrder here as they have separate methods

                    await _dbContext.SaveChangesAsync();

                    return existingModel.ModelId;
                }
                else
                {
                    // Insert new car model
                    // Calculate next sort order - safely handle empty tables
                    int nextSortOrder = 1; // Default to 1 if no records exist

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
                // Log the specific exception and re-throw it to be handled by the controller
                Console.WriteLine($"Validation error in InsertUpdateCarModelAsync: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception
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

                // Mark as deleted instead of physically deleting
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
                // Use a transaction to ensure consistency
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    foreach (var item in sortOrders)
                    {
                        var model = await _dbContext.TblCarModels.FirstOrDefaultAsync(m => m.ModelId == item.ModelId);
                        if (model != null)
                        {
                            model.Sortorder = item.SortOrder;
                        }
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
                // Generate a unique filename
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(_imageUploadPath, fileName);

                // Save the image file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Use a transaction to ensure data consistency
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // If this is the default image, update other images to non-default
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

                    // Save image metadata to database
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

                    // Delete the physical file if transaction failed
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
                // Use a transaction to ensure data consistency
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // If the first image will be default, update other images to non-default
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

                    // Process each image
                    for (int i = 0; i < images.Count; i++)
                    {
                        var image = images[i];
                        bool isDefault = setFirstAsDefault && i == 0;

                        // Generate a unique filename
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        string filePath = Path.Combine(_imageUploadPath, fileName);

                        // Save the image file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Save image metadata to database
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

                    // Clean up any files that were created
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

                // Delete the physical file if it exists
                if (!string.IsNullOrEmpty(image.ImageName))
                {
                    string filePath = Path.Combine(_imageUploadPath, image.ImageName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                // Remove from database
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
                    // Reset all images for this model to non-default
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

        // Helper method to get default image URL for a car model
        private async Task<string> GetDefaultImageUrlAsync(int modelId)
        {
            var defaultImage = await _dbContext.TblImages
                .FirstOrDefaultAsync(i => i.ModelId == modelId && i.IsDefault == true);

            if (defaultImage != null && !string.IsNullOrEmpty(defaultImage.ImageName))
            {
                return $"/images/cars/{defaultImage.ImageName}";
            }

            return "/images/cars/default-car.png"; // Default placeholder
        }
    }
}