using AMS.DtoLibrary.DTO.HolidayDto;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Entities.Infrastructure.Repository.IRepository;
using AMS.Entities.Models.Domain.Entities;
using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using AMS.Services.Utility.HelperMethods;
using AMS.Services.Utility.ResponseModel;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AMS.Services.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IGenericRepository<Holidays> _genericRepository;
        private readonly IGenericRepository<Leave> _leaveRepository;
        private readonly ICacheService _cacheService;
        private readonly IResponseService _responseService;
        private readonly ILogger<HolidayService> _logger;
        private readonly IMapper _mapper;
        private readonly ILeaveService _leaveService;
        public HolidayService(IGenericRepository<Holidays> genericRepository, ICacheService cacheService, 
            IResponseService responseService, ILogger<HolidayService> logger, IMapper mapper, ILeaveService leaveService, IGenericRepository<Leave> leaveRepository) 
        { 
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _leaveRepository = leaveRepository ?? throw new ArgumentNullException(nameof(leaveRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _leaveService = leaveService ?? throw new ArgumentNullException(nameof(leaveService));
        }
        
        public async Task<Response<HolidayCreationDto>> CreateNewHolidayAsync(HolidayCreationDto holidayCreationDto)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var holiday = _mapper.Map<Holidays>(holidayCreationDto);
            if(holiday == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error", new HolidayCreationDto());
            }
            var allHolidays = await _genericRepository.GetAllAsync();
            if(allHolidays.Any())
            {
                var isHolidayPresent = allHolidays.Where(x => x.Holiday.Date == holidayCreationDto.Holiday.Date).Any();
                if(isHolidayPresent)
                {
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Holiday already available", new HolidayCreationDto());
                }
            }
            var result = await _genericRepository.CreateAsync(holiday);
            if(result)
            {
                var holidayInserted = await _leaveService.ApplyLeavesForHolidaysAsync();
                if(holidayInserted)
                {
                    var holidaysList = await GetAllAsync();
                    var isSuccess = SetData(CacheKeys.Holiday, holidaysList);

                    if (isSuccess)
                    {
                        _logger.LogDebug($"Data set into Cache");
                    }
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.Created, "Success", holidayCreationDto);
                }
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error", new HolidayCreationDto());
        }

        public async Task<ResponseList<Holidays>> GetAllHolidaysAsync()
        {
            IEnumerable<Holidays> holidaysList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.Holiday);
            if(result.IsNullOrEmpty())
            {
                holidaysList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Holiday, holidaysList);

                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                holidaysList = result;
            }

            if (holidaysList == null || holidaysList.ToList().Count == 0)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync<Holidays>(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new List<Holidays>());
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync<Holidays>(true, (int)HttpStatusCode.OK, "Success", holidaysList);
        }

        public async Task<bool> DeleteAllHolidaysAsync(string year)
        {
            IEnumerable<Holidays> holidaysList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.Holiday);
            if (result.IsNullOrEmpty())
            {
                holidaysList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Holiday, holidaysList);

                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                holidaysList = result;
            }

            var toDeleteHolidays = holidaysList.Where(x => x.Holiday.Year.ToString() == year).ToList();
            if(toDeleteHolidays.IsNullOrEmpty())
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return false;
            }
            foreach (var holiday in toDeleteHolidays)
            {
                var isDeleted = await _genericRepository.DeleteAsync(holiday);
                if (isDeleted)
                {
                    var allleaves = await _leaveRepository.GetAllAsync();
                    var allLeavesToBeDeleted = allleaves.Where(x => x.LeaveStartDate.Date == holiday.Holiday.Date && x.LeaveType == "Holiday");
                    foreach (var leave in allLeavesToBeDeleted)
                    {
                        await _leaveRepository.DeleteAsync(leave);
                    }
                }
            }
            holidaysList = await GetAllAsync();
            var success = SetData(CacheKeys.Holiday, holidaysList);

            if (success)
            {
                _logger.LogDebug($"Data set into Cache");
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return true;
        }

        private async Task<IEnumerable<Holidays>> GetAllAsync()
        {
            return await _genericRepository.GetAllAsync();
        }

        private IEnumerable<Holidays> GetData(string key)
        {
            var cacheData = _cacheService.GetData<IEnumerable<Holidays>>(key);
            if (cacheData != null)
            {
                return cacheData;
            }
            return null;
        }

        private bool SetData(string key, IEnumerable<Holidays> data)
        {
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            var success = _cacheService.SetData<IEnumerable<Holidays>>(key, data, expirationTime);
            return success;
        }
    }
}
