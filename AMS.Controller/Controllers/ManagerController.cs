using AMS.Controller.Extensions;
using AMS.Services.Services.IServices;
using JwtAuthenticationManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace AMS.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _service;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(IManagerService service, ILogger<ManagerController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet(Name = "ReadAllManager")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReadAllManagerAsync()
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ReadAllManagerAsync();
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
    }
}
