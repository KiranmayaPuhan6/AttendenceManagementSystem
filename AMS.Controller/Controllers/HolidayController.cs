using AMS.Controller.Extensions;
using AMS.DtoLibrary.DTO.HolidayDto;
using AMS.Services.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AMS.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _service;
        private readonly ILogger<HolidayController> _logger;
        public HolidayController(IHolidayService service, ILogger<HolidayController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("CreateHoliday")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewHolidayAsync(HolidayCreationDto holidayCreationDto)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = await _service.CreateNewHolidayAsync(holidayCreationDto);
            if (!result.IsSuccess)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpGet("GetAllHolidays")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllHolidaysAsync()
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = await _service.GetAllHolidaysAsync();
            if (!result.IsSuccess)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpDelete("DeleteHolidaysForAYear")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllHolidaysAsync(string year)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = await _service.DeleteAllHolidaysAsync(year);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound("No such holiday found");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Deleted");
        }
    }
}
