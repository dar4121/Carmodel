using CarmodelAPI.Models;
using CarmodelAPI.Models.ViewModels;
using CarmodelAPI.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarmodelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarModelsController : ControllerBase
    {
        private readonly ICarModelRepository _carModelRepository;

        public CarModelsController(ICarModelRepository carModelRepository)
        {
            _carModelRepository = carModelRepository;
        }

        // GET: api/CarModels/Brands
        [HttpGet("Brands")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TblBrand>))]
        public async Task<ActionResult<IEnumerable<TblBrand>>> GetBrands()
        {
            var brands = await _carModelRepository.GetAllBrandsAsync();
            return Ok(brands);
        }

        // GET: api/CarModels/Classes
        [HttpGet("Classes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TblClass>))]
        public async Task<ActionResult<IEnumerable<TblClass>>> GetClasses()
        {
            var classes = await _carModelRepository.GetAllClassesAsync();
            return Ok(classes);
        }

        // GET: api/CarModels
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CarModelViewModel>))]
        public async Task<ActionResult<IEnumerable<CarModelViewModel>>> GetCarModels()
        {
            var models = await _carModelRepository.GetAllCarModelsAsync();
            return Ok(models);
        }

        // GET: api/CarModels/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarModelViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarModelViewModel>> GetCarModel(int id)
        {
            var model = await _carModelRepository.GetCarModelByIdAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }

        // POST: api/CarModels
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CarModelViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CarModelViewModel>> CreateCarModel([FromBody] CarModelCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var modelId = await _carModelRepository.InsertUpdateCarModelAsync(model);

                if (modelId == 0)
                {
                    return BadRequest("Failed to create car model");
                }

                var createdModel = await _carModelRepository.GetCarModelByIdAsync(modelId);
                return CreatedAtAction(nameof(GetCarModel), new { id = modelId }, createdModel);
            }
            catch (InvalidOperationException ex)
            {
                // Handle the specific exception for duplicate model code
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while creating the car model");
            }
        }

        // PUT: api/CarModels/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarModelViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarModelViewModel>> UpdateCarModel(int id, [FromBody] CarModelCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.ModelId)
            {
                return BadRequest("Model ID mismatch");
            }

            var existingModel = await _carModelRepository.GetCarModelByIdAsync(id);
            if (existingModel == null)
            {
                return NotFound();
            }

            try
            {
                var modelId = await _carModelRepository.InsertUpdateCarModelAsync(model);

                if (modelId == 0)
                {
                    return BadRequest("Failed to update car model");
                }

                var updatedModel = await _carModelRepository.GetCarModelByIdAsync(modelId);
                return Ok(updatedModel);
            }
            catch (InvalidOperationException ex)
            {
                // Handle the specific exception for duplicate model code
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while updating the car model");
            }
        }

        // DELETE: api/CarModels/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCarModel(int id)
        {
            var model = await _carModelRepository.GetCarModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var result = await _carModelRepository.DeleteCarModelAsync(id);

            if (!result)
            {
                return BadRequest("Failed to delete car model");
            }

            return NoContent();
        }

        // PUT: api/CarModels/UpdateSortOrder
        [HttpPut("UpdateSortOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSortOrder([FromBody] List<SortOrderUpdateViewModel> sortOrders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _carModelRepository.UpdateSortOrderAsync(sortOrders);

            if (!result)
            {
                return BadRequest("Failed to update sort order");
            }

            return Ok();
        }
    }
}