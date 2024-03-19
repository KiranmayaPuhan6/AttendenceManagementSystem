using AMS.Controller.Extensions;
using AMS.Controller.Validators.UserValidators;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Services.Services.IServices;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace AMS.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;
        private readonly JwtTokenHandler _jwtAuthenticationManager;

        public UserController(IUserService service, ILogger<UserController> logger, JwtTokenHandler jwtAuthenticationManager)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtAuthenticationManager = jwtAuthenticationManager ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("Authenticate")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginUser([FromBody] AuthenticationRequest _user)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var authenticationResponse =await _jwtAuthenticationManager.GenerateToken(_user);
            if (authenticationResponse == null)
            {
                _logger.LogError($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return Unauthorized();
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(authenticationResponse);
        }

        [HttpGet("Validate")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllUserAsync()
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.GetAllUsersAsync();
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpPost(Name = "CreateNewUser")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateNewUserAsync([FromForm] UserCreationDto userCreationDto)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            UserCreationDtoValidator validator = new UserCreationDtoValidator();
            var validation = validator.Validate(userCreationDto);
            if (!validation.IsValid)
            {
                _logger.LogError($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended with errors. {validation}");
                return BadRequest(validation);
            }
            var result = await _service.CreateNewUserAsync(userCreationDto);
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

        [HttpGet(Name = "ReadAllUser")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReadAllUserAsync()
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ReadAllUserAsync();
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpPut(Name = "UpdateUser")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateUserAsync([FromForm] UserUpdateDto userUpdateDto)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            UserUpdateDtoValidator validator = new UserUpdateDtoValidator();
            var validation = validator.Validate(userUpdateDto);
            if (!validation.IsValid)
            {
                _logger.LogError($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended with errors. {validation}");
                return BadRequest(validation);
            }

            var result = await _service.UpdateUserAsync(userUpdateDto);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpDelete("id/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.DeleteUserAsync(id);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound(result);
            }
            if (result.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest(result);
            }

            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpGet("id/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReadApiConfigResponseAsync(int id)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ReadUserAsync(id);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound(result);
            }
            _logger.LogDebug($"{result.Message} message with StatusCode: {result.StatusCode} from {MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name}");
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(result);
        }

        [HttpPost("VerificationCodeForEmail")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> VerificationCodeForEmailAsync(int userId)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.VerificationCodeForEmailAsync(userId);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest("Provide a valid email");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Verification code sent to registered emailaddress");
        }

        [HttpPost("VerifyEmail")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> VerifyEmailAsync(int userId, int token)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.VerifyEmailAsync(userId, token);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest("Token is incorrect.");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Email Verified");
        }

        [HttpPost("VerificationCodeForPhoneNumber")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> VerificationCodeForPhoneNumberAsync(int userId)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.VerificationCodeForPhoneNumberAsync(userId);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest("Provide a valid phone-number");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Verification code sent to registered phone-number");
        }

        [HttpPost("VerifyPhoneNumber")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> VerifyPhoneNumberAsync(int userId, int token)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.VerifyPhoneNumberAsync(userId, token);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest("Token is incorrect.");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("PhoneNumber Verified");
        }
    }
}
