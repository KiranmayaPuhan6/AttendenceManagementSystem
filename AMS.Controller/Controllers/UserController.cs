using AMS.Controller.Extensions;
using AMS.Controller.Validators.UserValidators;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using Castle.Core.Internal;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using JwtAuthenticationManager.Utility;
using Microsoft.AspNetCore.Authorization;
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
            var refreshToken = TokenUtils.GenerateRefreshToken();
            if (authenticationResponse == null)
            {
                _logger.LogError($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return Unauthorized("Email or password is not correct or email not verified");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
           authenticationResponse.RefreshToken = refreshToken;
            return Ok(authenticationResponse);
        }

        [HttpPost("refresh")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Employee,Manager")]
        public IActionResult Refresh(TokenResponse tokenResponse)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var response = TokenUtils.GenerateAccessTokenFromRefreshToken(tokenResponse);
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok(response);
        }

        [HttpGet("Validate/aec19c47-f1f4-4801-87a4-199f44443a5d")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
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
        [Authorize(Roles ="Admin")]
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
        [Authorize(Roles ="Admin,Employee,Manager")]
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


        [HttpPut("UpdateManager")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateManagerAsync(UserManagerUpdateDto userManagerUpdateDto)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = await _service.UpdateManagerAsync(userManagerUpdateDto);
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

        [HttpPut("UpdateRole")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRoleAsync(DtoLibrary.DTO.UserDto.UserRoleUpdateDto role)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = await _service.UpdateRoleAsync(role);
            if (result.IsNullOrEmpty())
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound("User not found");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Role changed");
        }

        [HttpDelete("id/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin,Employee,Manager")]
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
        [Authorize(Roles = "Admin,Employee,Manager")]
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

        [HttpPut("VerifyEmail")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Employee,Manager")]
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
        [Authorize(Roles = "Admin,Employee,Manager")]
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

        [HttpPut("VerifyPhoneNumber")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Employee,Manager")]
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

        [HttpPost("ForgetPassword")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ResetPasswordTokenAsync(string email)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ResetPasswordTokenAsync(email);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest("Provide a valid email");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Verification code sent to registered emailaddress");
        }

        [HttpPut("ResetPassword")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ResetPasswordAsync(ResetPassword data)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ResetPasswordAsync(data);
            if (!result)
            {
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return BadRequest("Verification code is incorrect.");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Password changed successfully");
        }

    }
}
