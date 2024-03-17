using AMS.DtoLibrary.DTO.LeaveDto;
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
    public class LeaveService : ILeaveService
    {
        private readonly IGenericRepository<Leave> _genericRepository;
        private readonly IGenericRepository<Attendence> _attendenceRepository;
        private readonly ICacheService _cacheService;
        private readonly IResponseService _responseService;
        private readonly ILogger<LeaveService> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Holidays> _holidayRepository;
        public LeaveService(IGenericRepository<Leave> genericRepository, IGenericRepository<Attendence> attendenceRepository, ICacheService cacheService, IResponseService responseService,
            ILogger<LeaveService> logger, IMapper mapper, IEmailService emailService, IGenericRepository<User> userRepository, IGenericRepository<Holidays> holidayRepository)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _attendenceRepository = attendenceRepository ?? throw new ArgumentNullException(nameof(attendenceRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _holidayRepository = holidayRepository ?? throw new ArgumentNullException(nameof(holidayRepository));
        }

        public async Task<Response<LeaveBaseDto>> ApplyLeaveAsync(LeaveCreationDto leaveCreationDto)
        {
            IEnumerable<Leave> leaveList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var holidays = _holidayRepository.GetAllAsync();
            var isPresentHoliday = holidays.Result.Where(x => x.Holiday.Date >= leaveCreationDto.LeaveStartDate.Date && x.Holiday.Date < leaveCreationDto.LeaveEndDate);
            if(isPresentHoliday.Any())
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, $"{isPresentHoliday.FirstOrDefault().Holiday.ToString("dd/MM/yyyy")} is a holiday", new LeaveBaseDto());
            }

            var leave = _mapper.Map<Leave>(leaveCreationDto);

            var result = GetData(CacheKeys.Leave);

            if (result.IsNullOrEmpty())
            {
                leaveList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Leave, leaveList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                leaveList = result;
            }
            for(var date = leave.LeaveStartDate.Date; date < leave.LeaveEndDate.Date; date = date.AddDays(1))
            {
                var appliedLeaveList = leaveList.Where(l => l.UserId == leave.UserId);
                var isAlreadyApplied = appliedLeaveList.Any(l => l.LeaveStartDate <= date && l.LeaveEndDate > date);
                if(isAlreadyApplied)
                {
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Leave Already Applied", new LeaveBaseDto());
                }
            }
           
            leave.NumberOfDaysLeave = (leave.LeaveEndDate - leave.LeaveStartDate).TotalDays;
            
            if (leave.StartHalfDay)
            {
                leave.NumberOfDaysLeave -= 0.5;
            }
            if (leave.NumberOfDaysLeave <= 0)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "End Date cannot be less than Start Date", new LeaveBaseDto());
            }

            leave.TotalLeavesTaken = (leaveList.Where(l => l.UserId == leave.UserId && l.LeaveStartDate.Year == DateTime.Now.Year )
             .Sum(l => l.NumberOfDaysLeave));
            leave.TotalLeavesLeft = 15 - leave.TotalLeavesTaken;

            if (DateTime.Now.Month == 1 && DateTime.Now.Day == 1)
            {
                leave.TotalLeavesTaken = 0;
                leave.TotalLeavesLeft = 15;
            }

            if (leave.NumberOfDaysLeave > leave.TotalLeavesLeft)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Number of leaves exceeds totalLeavesLeft", new LeaveBaseDto());
            }

            if (leave.TotalLeavesLeft < 0)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "No leaves left", new LeaveBaseDto());
            }

            leave.IsApproved = false;
            leave.LeaveType = "Leave";

            var success = await _genericRepository.CreateAsync(leave);

            if (success)
            {
                leaveList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Leave, leaveList);

                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                var leaveBaseDto = _mapper.Map<LeaveBaseDto>(leave);
                var email = await GetEmailAsync(leave.UserId);
                if (email != null)
                {
                    EmailAddress userEmailAddress = new EmailAddress
                    {
                        To = email.Email ,
                        Subject = $"Leave Applied Successfully",
                        Message = $"<html><body><p>Hi {email.Name},</p><p>Your leave from {leave.LeaveStartDate.ToString("dd/MM/yyyy")} to {leave.LeaveEndDate.ToString("dd/MM/yyyy")} has been applied successfully." +
                        $"</p><p>Number of Leaves taken = {leave.NumberOfDaysLeave}. It is pending at {email.ManagerName} for approval.</p>" +
                        $"<p> Thanks,</p><p> AMS Team.</p></body></html>",
                    };
                    await _emailService.SendEmailAsync(userEmailAddress);

                    EmailAddress managerEmailAddress = new EmailAddress
                    {
                        To = email.ManagerEmail,
                        Subject = $"Leave Approval",
                        Message = $"<html><body><p>Hi {email.ManagerName},</p><p>{email.Name} has applied leave from {leave.LeaveStartDate.ToString("dd/MM/yyyy")} to {leave.LeaveEndDate.ToString("dd/MM/yyyy")}." +
                        $"</p><p>Approve the leave by visiting AMS.</p>" +
                        $"<p> Thanks,</p><p> AMS Team.</p></body></html>",
                    };
                    await _emailService.SendEmailAsync(managerEmailAddress);
                }

                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.Created, "Leave applied successfully", leaveBaseDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Leave not applied", new LeaveBaseDto());
        }

        public async Task<Response<LeaveDto>> ApproveLeaveAsync(LeaveUpdateDto leaveUpdateDto)
        {
            IEnumerable<Leave> leaveList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var leave = await _genericRepository.GetByIdAsync(leaveUpdateDto.Id);

            if (leave == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Leave not found", new LeaveDto());
            }

            leave.IsApproved = leaveUpdateDto.IsApproved;

            var email = await GetEmailAsync(leave.UserId);

            if (leave.IsApproved)
            {
                var result = GetData(CacheKeys.Leave);

                if (result.IsNullOrEmpty())
                {
                    leaveList = await GetAllAsync();
                    var isSuccess = SetData(CacheKeys.Leave, leaveList);
                    if (isSuccess)
                    {
                        _logger.LogDebug($"Data set into Cache");
                    }
                }
                else
                {
                    leaveList = result;
                }
                leave.TotalLeavesTaken = leaveList
                   .Where(l => l.UserId == leave.UserId && l.LeaveStartDate.Year == DateTime.Now.Year && l.IsApproved)
                   .Sum(l => l.NumberOfDaysLeave) + leave.NumberOfDaysLeave;
                leave.TotalLeavesLeft = 15 - leave.TotalLeavesTaken;

                if (leave.TotalLeavesLeft < 0)
                {
                    if (email != null)
                    {
                        EmailAddress userEmailAddress = new EmailAddress
                        {
                            To = email.Email,
                            Subject = $"Leave Not Approved",
                            Message = $"<html><body><p>Hi {email.Name},</p><p>Your leave from {leave.LeaveStartDate.ToString("dd/MM/yyyy")} to {leave.LeaveEndDate.ToString("dd/MM/yyyy")} is not approved." +
                            $"</p><p>You don't gave enough leaves .</p>" +
                            $"<p>You total leave count is {leave.TotalLeavesLeft}.</p>" +
                            $"<p> Thanks,</p><p> AMS Team.</p></body></html>",
                        };
                        await _emailService.SendEmailAsync(userEmailAddress);
                    }
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Leave not applied", new LeaveDto());
                }
                var success = await _genericRepository.UpdateAsync(leave);
                if (success)
                {                 
                   
                    var attendenceList = GetALlAttendenceByUserIdAsync(leave.UserId);
                    if(leave.StartHalfDay)
                    {
                        var endDate = leave.LeaveEndDate.Date.AddDays(-1);
                        var halfDayttendenceRecord = new Attendence
                        {
                            LoginTime = endDate.AddHours(9),
                            LogoutTime = endDate.AddHours(13),
                            TotalLoggedInTime = 4,
                            AttendenceType = "Leave",
                            UserId = leave.UserId
                        };

                        await _attendenceRepository.CreateAsync(halfDayttendenceRecord);
                        for (var date = leave.LeaveStartDate.Date; date < endDate; date = date.AddDays(1))
                        {
                            var isPresent = attendenceList.Result.Where(x => x.LoginTime.Date == date && x.AttendenceType == "Regular" && x.TotalLoggedInTime >= 5);
                            foreach (var item in isPresent)
                            {
                                await _attendenceRepository.DeleteAsync(item);
                            }
                            var attendenceRecord = new Attendence
                            {
                                LoginTime = date.AddHours(9),
                                LogoutTime = date.AddHours(17),
                                TotalLoggedInTime = 8,
                                AttendenceType = "Leave",
                                UserId = leave.UserId
                            };

                            await _attendenceRepository.CreateAsync(attendenceRecord);
                        }
                    }
                    
                    var leaveDto = _mapper.Map<LeaveDto>(leave);
                    if (email != null)
                    {
                        EmailAddress userEmailAddress = new EmailAddress
                        {
                            To = email.Email,
                            Subject = $"Leave Approved",
                            Message = $"<html><body><p>Hi {email.Name},</p><p>Your leave from {leave.LeaveStartDate.ToString("dd/MM/yyyy")} to {leave.LeaveEndDate.ToString("dd/MM/yyyy")} is approved." +
                            $"<p> Thanks,</p><p> AMS Team.</p></body></html>",
                        };
                        await _emailService.SendEmailAsync(userEmailAddress);
                    }
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.OK, "Leave approved", leaveDto);
                }
                var leaveNotApprovedDto = _mapper.Map<LeaveDto>(leave);
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Leave not approved", leaveNotApprovedDto);
            }
            if (email != null)
            {
                EmailAddress userEmailAddress = new EmailAddress
                {
                    To = email.Email,
                    Subject = $"Leave Not Approved",
                    Message = $"<html><body><p>Hi {email.Name},</p><p>Your leave from {leave.LeaveStartDate.ToString("dd/MM/yyyy")} to {leave.LeaveEndDate.ToString("dd/MM/yyyy")} is not approved." +
                    $"</p><p>Please connect with your manager.</p>" +
                    $"<p> Thanks,</p><p> AMS Team.</p></body></html>",
                };
                await _emailService.SendEmailAsync(userEmailAddress);
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.OK, "Leave not approved", new LeaveDto());
        }

        public async Task<Response<LeaveDto>> DeleteLeaveAsync(int id)
        {

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var leave = await _genericRepository.GetByIdAsync(id);

            if (leave == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new LeaveDto());
            }

            var leaveDto = _mapper.Map<LeaveDto>(leave);

            var result = await _genericRepository.DeleteAsync(leave);

            if (result)
            {
                var leaveList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Leave, leaveList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.NoContent, "Deleted", leaveDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Deletion Unsuccessful", new LeaveDto());
        }

        public async Task<ResponseList<LeaveDto>> GetAllLeavesAsync()
        {
            IEnumerable<Leave> leaveList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.Leave);

            if (result.IsNullOrEmpty())
            {
                leaveList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.Leave, leaveList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                leaveList = result;
            }
            var leaveDtoList = _mapper.Map<List<LeaveDto>>(leaveList).ToList();

            if (leaveDtoList == null || leaveDtoList.Count == 0)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync<LeaveDto>(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new List<LeaveDto>());
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync<LeaveDto>(true, (int)HttpStatusCode.OK, "Success", leaveDtoList);
        }

        public async Task<bool> ApplyLeavesForHolidaysAsync()
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
                var holidays = await _holidayRepository.GetAllAsync();
                if (holidays.Any())
                {
                    var users = await _userRepository.GetAllAsync();
                    foreach (var user in users)
                    {
                        var leavesList = await _genericRepository.GetAllAsync();

                        foreach (var holiday in holidays)
                        {
                            var isPresent = leavesList.Where(x => x.LeaveStartDate == holiday.Holiday && x.UserId == user.UserID && x.LeaveType == "Holiday").Any();
                            if(isPresent)
                            {
                                continue;
                            }
                            Leave leave = new Leave
                            {
                                LeaveStartDate = holiday.Holiday,
                                LeaveEndDate = holiday.Holiday.AddDays(1),
                                StartHalfDay = false,
                                IsApproved = true,
                                LeaveType = "Holiday",
                                UserId = user.UserID
                            };
                             await _genericRepository.CreateAsync(leave);

                             var attendenceRecord = new Attendence
                             {
                                 LoginTime = holiday.Holiday.AddHours(9),
                                 LogoutTime = holiday.Holiday.AddHours(17),
                                 TotalLoggedInTime = 8,
                                 AttendenceType = "Holiday",
                                 UserId = user.UserID
                             };
                            await _attendenceRepository.CreateAsync(attendenceRecord);
                        }
                    }
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return true;
                }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return false;
        }
        private async Task<IEnumerable<Leave>> GetAllAsync()
        {
            return await _genericRepository.GetAllAsync();
        }
        private IEnumerable<Leave> GetData(string key)
        {
            var cacheData = _cacheService.GetData<IEnumerable<Leave>>(key);
            if (cacheData != null)
            {
                return cacheData;
            }
            return null;
        }

        private bool SetData(string key, IEnumerable<Leave> data)
        {
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            var success = _cacheService.SetData<IEnumerable<Leave>>(key, data, expirationTime);
            return success;
        }

        private async Task<EmailDetails> GetEmailAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            EmailDetails emailDetails = new EmailDetails
            {
                Name = user.Name,
                Email = user.Email,
                ManagerEmail = user.ManagerEmail,
                ManagerName = user.ManagerName,
            };
            
            return emailDetails;
        }

        private async Task<IEnumerable<Attendence>> GetALlAttendenceByUserIdAsync(int userId)
        {
            var attendenceList = await _attendenceRepository.GetAllAsync();
            return attendenceList.Where(x => x.UserId == userId).ToList();
        }
    }
}
