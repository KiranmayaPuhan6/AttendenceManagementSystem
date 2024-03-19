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
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _genericRepository;
        private readonly ICacheService _cacheService;
        private readonly IResponseService _responseService;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        public UserService(IGenericRepository<User> genericRepository, ICacheService cacheService, IResponseService responseService, 
            ILogger<UserService> logger, IMapper mapper, IImageService imageService, IEmailService emailService, ISmsService smsService)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        }

        public async Task<Response<UserBaseDto>> CreateNewUserAsync(UserCreationDto userCreationDto)
        {
            string path;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var allUsers = await GetAllAsync();
            var isEmailPresent = allUsers.Where(x => x.Email == userCreationDto.Email).Any();
            if(isEmailPresent )
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Email already taken", new UserBaseDto());
            }
            var user = _mapper.Map<User>(userCreationDto);
            if (user.FileUri != null)
            {
                path = await _imageService.UploadImageAsync(user.FileUri);
                if (path == "Not a valid type")
                {
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error image type", new UserBaseDto());
                }
            }
            else
            {
                path = null;
            }
            var userModel = new User()
            {
                Company = user.Company,
                Designation = user.Designation,
                ManagerName = user.ManagerName,
                ManagerEmail = user.ManagerEmail,
                Address = user.Address,
                Gender = user.Gender,
                Email = user.Email,
                IsEmailConfirmed = false,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Role = user.Role,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                IsPhoneNumberConfirmed = false,
                ActualFileUrl = path
            };
            var result = await _genericRepository.CreateAsync(userModel);

            if (result)
            {
                var userList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.User, userList);

                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                var userBaseDto = _mapper.Map<UserBaseDto>(user);
                userBaseDto.ActualFileUrl = path;
                EmailAddress emailAddress = new EmailAddress
                {
                    To = user.Email,
                    Subject = $"Registration Successful",
                    Message = $"<html><body><p>Hi {user.Name},</p><p>Thanks for registering to Attendence Management System.</p><p> Have a nice day.</p><p> Thanks,</p><p> AMS Team.</p></body></html>",
                };
                await _emailService.SendEmailAsync(emailAddress);

                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");

                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.Created, "Success", userBaseDto);
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");

            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error", new UserBaseDto());
        }

        public async Task<ResponseList<UserDto>> ReadAllUserAsync()
        {
            IEnumerable<User> userList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.User);

            if (result.IsNullOrEmpty())
            {
                userList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.User, userList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                userList = result;
            }
            var userDtoList = _mapper.Map<List<UserDto>>(userList).ToList();

            if (userDtoList == null || userDtoList.Count == 0)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync<UserDto>(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new List<UserDto>());
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync<UserDto>(true, (int)HttpStatusCode.OK, "Success", userDtoList);
        }

        public async Task<Response<UserBaseDto>> UpdateUserAsync(UserUpdateDto userUpdateDto)
        {
            string path;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var userInfo = await _genericRepository.GetByIdAsync(userUpdateDto.UserId);
            if (userInfo == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "No such user", new UserBaseDto());
            }
            var user = _mapper.Map<User>(userUpdateDto);
            if (user.FileUri != null)
            {
                if (userInfo.ActualFileUrl != null)
                {
                   var deletionResult = await _imageService.DeleteImageAsync(userInfo.ActualFileUrl);
                    if (deletionResult == null)
                    {
                        _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                        return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Image Deletion Unsuccessful", new UserBaseDto());
                    }
                }
                path = await _imageService.UploadImageAsync(user.FileUri);
                if (path == "Not a valid type")
                {
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Error image type", new UserBaseDto());
                }
            }
            else
            {
                path = null;
            }
            var userModel = new User()
            {
                UserID = userUpdateDto.UserId,
                Company = user.Company,
                Designation = user.Designation,
                ManagerName = user.ManagerName,
                ManagerEmail = user.ManagerEmail,
                Address = user.Address,
                Gender = user.Gender,
                Email = user.Email,
                Password = userInfo.Password,
                Role = user.Role,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                ActualFileUrl = path
            };
            var result = await _genericRepository.UpdateAsync(userModel);

            if (result)
            {
                var userList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.User, userList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }

                var userBaseDto = _mapper.Map<UserBaseDto>(userModel);

                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.NoContent, "Updated", userBaseDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "Error", new UserBaseDto());
        }

        public async Task<Response<UserDto>> DeleteUserAsync(int id)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var user = await _genericRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new UserDto());
            }

            var userDto = _mapper.Map<UserDto>(user);

            var result = await _genericRepository.DeleteAsync(user);

            if (result)
            {
                if(user.ActualFileUrl != null)
                {
                    var deletionImage = await _imageService.DeleteImageAsync(user.ActualFileUrl);
                    if (deletionImage == null)
                    {
                        _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                        return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Deletion Unsuccessful", new UserDto());
                    }
                }
                var userList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.User, userList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.NoContent, "Deleted", userDto);
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.BadRequest, "Deletion Unsuccessful", new UserDto());
        }

        public async Task<Response<UserDto>> ReadUserAsync(int id)
        {
            IEnumerable<User> userList;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var result = GetData(CacheKeys.User);

            if (result.IsNullOrEmpty())
            {
                userList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.User, userList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }
            else
            {
                userList = result;
            }

            var userDto = _mapper.Map<UserDto>(userList?.FirstOrDefault(x => x.UserID == id));

            if (userDto == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "RecordsNotFound", new UserDto());
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.OK, "Success", userDto);
        }

        public async Task<bool> VerifyEmailAsync(int userId, int token)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");

            var user = await _genericRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            if (user.PasswordResetToken == token && DateTime.Now < user.ResetTokenExpires)
            {
                user.PasswordResetToken = null;
                user.ResetTokenExpires = null;
                user.IsEmailConfirmed = true;
                var success = await _genericRepository.UpdateAsync(user);
                if(success)
                {
                    var userList = await GetAllAsync();
                    var isSuccess = SetData(CacheKeys.User, userList);

                    if (isSuccess)
                    {
                        _logger.LogDebug($"Data set into Cache");
                    }
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return true;
                }
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return false;
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return false;
        }

        public async Task<bool> VerificationCodeForEmailAsync(int userId)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var generatedNumber = RandomNumberGenerator.Generate(100000, 999999);
            var user = await _genericRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return false;
            }
            user.PasswordResetToken = generatedNumber;
            user.ResetTokenExpires = DateTime.Now.AddMinutes(30);
            EmailAddress emailAddress = new EmailAddress
            {
                To = user.Email,
                Subject = "Verify your email",
                Message = $"<html><body><p>Your verification code is {generatedNumber}.</p><p>It is valid for 30 minutes.</p></body></html>"
            };
            await _emailService.SendEmailAsync(emailAddress);
            var success = await _genericRepository.UpdateAsync(user);
            if (success)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return true;
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return false;
        }

        public async Task<bool> VerifyPhoneNumberAsync(int userId, int token)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var user = await _genericRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return false;
            }
            if (user.PasswordResetToken == token && DateTime.Now < user.ResetTokenExpires)
            {
                user.PasswordResetToken = null;
                user.ResetTokenExpires = null;
                user.IsPhoneNumberConfirmed = true;
                var success = await _genericRepository.UpdateAsync(user);
                if (success)
                {
                    var userList = await GetAllAsync();
                    var isSuccess = SetData(CacheKeys.User, userList);

                    if (isSuccess)
                    {
                        _logger.LogDebug($"Data set into Cache");
                    }
                    _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                    return true;
                }
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return false;
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return false;
        }

        public async Task<bool> VerificationCodeForPhoneNumberAsync(int userId)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var generatedNumber = RandomNumberGenerator.Generate(100000, 999999);
            var user = await _genericRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return false;
            }
            user.PasswordResetToken = generatedNumber;
            user.ResetTokenExpires = DateTime.Now.AddMinutes(30);
            await _smsService.SendMessageAsync(user.PhoneNumber,$"Your verification code is {generatedNumber} . This is valid for 30 mins.");
            var success = await _genericRepository.UpdateAsync(user);
            if (success)
            {
                _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
                return true;
            }
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return false;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var users = await _genericRepository.GetAllAsync();
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return users;
        }

        private async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _genericRepository.GetAllAsync(predicate: null,includes: q => q.Include(u => u.Attendence).Include(u => u.Leave));
        }

        private IEnumerable<User> GetData(string key)
        {
            var cacheData = _cacheService.GetData<IEnumerable<User>>(key);
            if (cacheData != null)
            {
                return cacheData;
            }
            return null;
        }

        private bool SetData(string key, IEnumerable<User> data)
        {
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            var success = _cacheService.SetData<IEnumerable<User>>(key, data, expirationTime);
            return success;
        }

    }
}
