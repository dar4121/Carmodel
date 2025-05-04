using CarmodelAPI.Models.ViewModels;
using CarmodelAPI.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CarmodelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarModelImagesController : ControllerBase
    {
        private readonly ICarModelRepository _carModelRepository;

        public CarModelImagesController(ICarModelRepository carModelRepository)
        {
            _carModelRepository = carModelRepository;
        }

        // GET: api/CarModelImages/model/5
        [HttpGet("model/{modelId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ImageViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ImageViewModel>>> GetCarModelImages(int modelId)
        {
            var model = await _carModelRepository.GetCarModelByIdAsync(modelId);
            if (model == null)
            {
                return NotFound("Car model not found");
            }

            var images = await _carModelRepository.GetCarModelImagesAsync(modelId);
            return Ok(images);
        }

        // GET: api/CarModelImages/model/5/default
        [HttpGet("model/{modelId}/default")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImageViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ImageViewModel>> GetDefaultImage(int modelId)
        {
            var model = await _carModelRepository.GetCarModelByIdAsync(modelId);
            if (model == null)
            {
                return NotFound("Car model not found");
            }

            var images = await _carModelRepository.GetCarModelImagesAsync(modelId);
            var defaultImage = images.FirstOrDefault(img => img.IsDefault);

            if (defaultImage == null)
            {
                return NotFound("No default image found");
            }

            return Ok(defaultImage);
        }

        // POST: api/CarModelImages/Upload
        [HttpPost("Upload")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UploadImages([FromForm] ImageUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var carModel = await _carModelRepository.GetCarModelByIdAsync(model.ModelId);
            if (carModel == null)
            {
                return NotFound("Car model not found");
            }

            // Handle multiple image upload
            if (model.Images.Count > 0)
            {
                var imageIds = await _carModelRepository.UploadCarModelImagesAsync(
                    model.ModelId, model.Images, model.IsDefault);

                if (imageIds.Count == 0)
                {
                    return BadRequest("Failed to upload images");
                }

                return CreatedAtAction(nameof(GetCarModelImages), new { modelId = model.ModelId }, null);
            }
            // Fallback to single image upload for backward compatibility
            else if (model.Image != null)
            {
                var imageId = await _carModelRepository.UploadCarModelImageAsync(
                    model.ModelId, model.Image, model.IsDefault);

                if (imageId == 0)
                {
                    return BadRequest("Failed to upload image");
                }

                return CreatedAtAction(nameof(GetCarModelImages), new { modelId = model.ModelId }, null);
            }
            else
            {
                return BadRequest("No images provided for upload");
            }
        }

        // DELETE: api/CarModelImages/5
        [HttpDelete("{imageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteImage(int imageId)
        {
            var result = await _carModelRepository.DeleteCarModelImageAsync(imageId);

            if (!result)
            {
                return NotFound("Image not found or could not be deleted");
            }

            return NoContent();
        }

        // PUT: api/CarModelImages/SetDefaultImage/5/model/10
        [HttpPut("SetDefaultImage/{imageId}/model/{modelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetDefaultImage(int imageId, int modelId)
        {
            var carModel = await _carModelRepository.GetCarModelByIdAsync(modelId);
            if (carModel == null)
            {
                return NotFound("Car model not found");
            }

            var result = await _carModelRepository.SetDefaultImageAsync(imageId, modelId);

            if (!result)
            {
                return BadRequest("Failed to set default image");
            }

            return Ok();
        }
    }
}