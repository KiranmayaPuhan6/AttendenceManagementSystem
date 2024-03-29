﻿using AMS.Controller.Extensions;
using AMS.DtoLibrary.DTO.LeaveDto;
using AMS.Services.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace AMS.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _service;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(ILeaveService service, ILogger<LeaveController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpPost("ApplyLeave")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> ApplyLeaveAsync(LeaveCreationDto leaveCreationDto)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = await _service.ApplyLeaveAsync(leaveCreationDto);
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

        [HttpPut("ApproveLeave")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveLeaveAsync(LeaveUpdateDto leaveUpdateDto)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ApproveLeaveAsync(leaveUpdateDto);
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

        [HttpDelete("id/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteLeaveAsync(int id)
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.DeleteLeaveAsync(id);
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

        [HttpGet("GetAllLeaves")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLeavesAsync()
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.GetAllLeavesAsync();
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

        [HttpPost("ApplyLeaveForHolidays")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApplyLeavesForHolidaysAsync()
        {
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var result = await _service.ApplyLeavesForHolidaysAsync();
            if (!result)
            { 
                _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return NotFound("No holidays for this year");
            }
            _logger.LogInformation($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return Ok("Time sheet for holidays has been applied");
        }

    }
}
