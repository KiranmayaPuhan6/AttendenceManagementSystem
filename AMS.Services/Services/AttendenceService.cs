using AMS.DtoLibrary.DTO.AttendenceDto;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Entities.Infrastructure.Repository.IRepository;
using AMS.Entities.Models.Domain.Entities;
using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using AMS.Services.Utility.HelperMethods;
using AMS.Services.Utility.ResponseModel;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AMS.Services.Services
{
    public class AttendenceService : IAttendenceService
    {
        private readonly IGenericRepository<Attendence> _genericRepository;
        private readonly ICacheService _cacheService;
        private readonly IResponseService _responseService;
        private readonly ILogger<AttendenceService> _logger;
        private readonly IMapper _mapper;
        private readonly ISmsService _smsService;
        private readonly IGenericRepository<User> _userRepository;
        public AttendenceService(IGenericRepository<Attendence> genericRepository, ICacheService cacheService, IResponseService responseService,
            ILogger<AttendenceService> logger, IMapper mapper, ISmsService smsService, IGenericRepository<User> userRepository)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _smsService = smsService;
            _userRepository = userRepository;
        }

        public async Task<Response<AttendenceBaseDto>> AttendenceLogIn(int userId)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var attendance = new Attendence
            {
                UserId = userId,
                LoginTime = DateTime.Now,
                AttendenceType = "Regular"
            };

            var result = await _genericRepository.CreateAsync(attendance);

            if (result)
            {
                var attendenceList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Attendence, attendenceList);

                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                var attendenceBaseDto = _mapper.Map<AttendenceBaseDto>(attendance);
                var phoneNumber =await GetPhoneNumberAsync(userId);
                if (phoneNumber != null)
                {
                   await _smsService.SendMessageAsync(phoneNumber, $"Your Login Time for Date -: {attendance.LoginTime.ToString("dd/MM/yyyy")} is {attendance.LoginTime.ToString("HH:mm:ss")} .");
                }

                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.Created, "Logged in successfully", attendenceBaseDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error logging in", new AttendenceBaseDto());
        }

        public async Task<Response<AttendenceBaseDto>> AttendenceLogOut(int userId)
        {
            IEnumerable<Attendence> attendenceList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.Attendence);

            if (result.IsNullOrEmpty())
            {
                attendenceList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Attendence, attendenceList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                attendenceList = result;
            }
                                 
            var attendance = (attendenceList?.FirstOrDefault(a => a.UserId == userId && a.LogoutTime == null));
            if (attendance == null)
            {
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "User not found or already logged out.", new AttendenceBaseDto());
            }

            attendance.LogoutTime = DateTime.Now;

            if (attendance.LogoutTime.HasValue)
            {
                attendance.TotalLoggedInTime = Math.Round((attendance.LogoutTime.Value - attendance.LoginTime).TotalHours,2);
            }
            else
            {
                attendance.TotalLoggedInTime = 0;
            }

            var success = await _genericRepository.UpdateAsync(attendance);

            if (success)
            {
                attendenceList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Attendence, attendenceList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                var attendenceBaseDto = _mapper.Map<AttendenceBaseDto>(attendance);
                var phoneNumber = await GetPhoneNumberAsync(userId);
                if (phoneNumber != null)
                {
                   await _smsService.SendMessageAsync(phoneNumber, $"Your Logout Time for Date -: {attendance.LogoutTime.Value.ToString("dd/MM/yyyy")} is {attendance.LogoutTime.Value.ToString("HH:mm:ss")} .");
                }

                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.OK, "Logged out successfully.", attendenceBaseDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error logging out", new AttendenceBaseDto());
        }

        public async Task<ResponseList<AttendenceDto>> GetAllAttendenceAsync()
        {
            IEnumerable<Attendence> atendenceList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.Attendence);

            if (result.IsNullOrEmpty())
            {
                atendenceList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Attendence, atendenceList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                atendenceList = result;
            }
            var atendenceDtoList = _mapper.Map<List<AttendenceDto>>(atendenceList).ToList();

            if (atendenceDtoList == null || atendenceDtoList.Count == 0)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync<AttendenceDto>(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new List<AttendenceDto>());
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync<AttendenceDto>(true, (int)HttpStatusCode.OK, "Success", atendenceDtoList);
        }

        public async Task<Response<AttendenceDto>> DeleteAttendenceAsync(int id)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var attendence = await _genericRepository.GetByIdAsync(id);

            if (attendence == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new AttendenceDto());
            }

            var attendenceDto = _mapper.Map<AttendenceDto>(attendence);

            var result = await _genericRepository.DeleteAsync(attendence);

            if (result)
            {
                var attendenceList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Attendence, attendenceList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.NoContent, "Deleted", attendenceDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Deletion Unsuccessful", new AttendenceDto());
        }

        private async Task<IEnumerable<Attendence>> GetAllAsync()
        {
            return await _genericRepository.GetAllAsync();
        }

        private IEnumerable<Attendence> GetData(string key)
        {
            var cacheData = _cacheService.GetData<IEnumerable<Attendence>>(key);
            if (cacheData != null)
            {
                return cacheData;
            }
            return null;
        }

        private bool SetData(string key, IEnumerable<Attendence> data)
        {
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            var success = _cacheService.SetData<IEnumerable<Attendence>>(key, data, expirationTime);
            return success;
        }

        private async Task<string> GetPhoneNumberAsync(int  userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user.PhoneNumber;
        }

    }
}
