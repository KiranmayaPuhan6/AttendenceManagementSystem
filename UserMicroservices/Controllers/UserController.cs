using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using UserMicroservices.Extensions;
using UserMicroservices.Models.Domain.Entities;
using UserMicroservices.Models.DTO;
using UserMicroservices.Services.IServices;
using UserMicroservices.Utility.ResponseModel;
using UserMicroservices.Validators;

namespace UserMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService service, ILogger<UserController> logger, IMapper mapper)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost(Name = "CreateNewUser")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateNewUserAsync([FromForm]UserCreationDto userCreationDto)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            UserCreationDtoValidator validator = new UserCreationDtoValidator();
            var validation = validator.Validate(userCreationDto);
            if (!validation.IsValid)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended with errors. {validation}");
                return BadRequest(validation);
            }
            var result = await _service.CreateNewUserAsync(userCreationDto);
            if(!result.IsSuccess)
            {
                _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                return BadRequest(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            return Ok(result);
        }

        [HttpGet(Name = "ReadAllUser")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReadAllUserAsync()
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ReadAllUserAsync();
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                return NotFound(result);
            }
             _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            return Ok(result);
        }

        [HttpPut(Name = "UpdateUser")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateUserAsync([FromForm]UserUpdateDto userUpdateDto)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            UserUpdateDtoValidator validator = new UserUpdateDtoValidator();
            var validation = validator.Validate(userUpdateDto);
            if (!validation.IsValid)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended with errors. {validation}");
                return BadRequest(validation);
            }

            var result = await _service.UpdateUserAsync(userUpdateDto);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                return NotFound(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            return Ok(result);
        }

        [HttpDelete("id/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.DeleteUserAsync(id);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                return NotFound(result);
            }
  
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            return Ok(result);
        }

        [HttpGet("id/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReadApiConfigResponseAsync(int id)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ReadUserAsync(id);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                return NotFound(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            return Ok(result);
        }
    }
}
