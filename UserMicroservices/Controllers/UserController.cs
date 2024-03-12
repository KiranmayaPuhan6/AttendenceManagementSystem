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
        private readonly IMapper _mapper;

        public UserController(IUserService service, ILogger<UserController> logger, IMapper mapper)
        {
            _service = service;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost(Name = "CreateNewUser")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateNewUserAsync([FromForm]UserCreationDto userCreationDto)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            UserValidator validator = new UserValidator();
            var user = _mapper.Map<User>(userCreationDto);
            var validation = validator.Validate(user);
            if (!validation.IsValid)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended with errors. {validation}");
                return BadRequest(validation);
            }
            var result = await _service.CreateNewUserAsync(userCreationDto);
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            return Ok(result);
        }

        [HttpGet(Name = "ReadAllUser")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReadAllUserAsync()
        {
            var result = await _service.ReadAllUserAsync();
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
            var result = await _service.DeleteUserAsync(id);
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
