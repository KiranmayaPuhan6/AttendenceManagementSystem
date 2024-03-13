using AutoMapper;
using Castle.Core.Internal;
using System.IO;
using System.Net;
using UserMicroservices.Data;
using UserMicroservices.Extensions;
using UserMicroservices.Models.Domain.Entities;
using UserMicroservices.Models.DTO;
using UserMicroservices.Repository.IRepository;
using UserMicroservices.Services.IServices;
using UserMicroservices.Utility;
using UserMicroservices.Utility.ResponseModel;

namespace UserMicroservices.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _genericRepository;
        private readonly ICacheService _cacheService;
        private readonly IResponseService _responseService;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        public UserService(IGenericRepository<User> genericRepository, ICacheService cacheService, IResponseService responseService, 
            ILogger<UserService> logger, IMapper mapper, IWebHostEnvironment environment, IConfiguration config)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<Response<UserBaseDto>> CreateNewUserAsync(UserCreationDto userCreationDto)
        {
            string path;
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var user = _mapper.Map<User>(userCreationDto);
            if (user.FileUri != null)
            {
                path = await UploadImage(user.FileUri);
                if (path == "Not a valid type")
                {
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
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Role = user.Role,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
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
                return await _responseService.ResponseDtoFormatterAsync(false, (int)HttpStatusCode.NotFound, "No such user", new UserBaseDto());
            }
            var user = _mapper.Map<User>(userUpdateDto);
            if (user.FileUri != null)
            {
                if (userInfo.ActualFileUrl != null)
                {
                    DeleteImage(userInfo.ActualFileUrl);
                }
                path = await UploadImage(user.FileUri);
                if (path == "Not a valid type")
                {
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
                    DeleteImage(user.ActualFileUrl);
                }
                var userList = await GetAllAsync();
                var isSuccess = SetData(CacheKeys.User, userList);
                if (isSuccess)
                {
                    _logger.LogDebug($"Data set into Cache");
                }
            }

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return await _responseService.ResponseDtoFormatterAsync(true, (int)HttpStatusCode.NoContent, "Deleted", userDto);
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

        private async Task<string> UploadImage(IFormFile objfile)
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                string[] allowedExtension = new string[] { ".jpg", ".jpeg", ".png", ".jfif" };
                if (objfile.Length > 0)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\Upload\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\Upload\\");
                    }
                    string extension = Path.GetExtension(objfile.FileName);
                    if ((allowedExtension.Contains(extension)))
                    {
                        using (FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\Upload\\" + guid + objfile.FileName))
                        {
                            objfile.CopyTo(fileStream);
                            fileStream.Flush();
                            return "\\Upload\\" + guid + objfile.FileName;
                        }
                    }
                    else
                    {
                        return "Not a valid type";
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        public async Task<string> DeleteImage(string path)
        {
            FileInfo fileInfo = new FileInfo(_environment.WebRootPath + path);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                return "Deleted";
            }
            else
            {
                return null;
            }
        }

        private async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _genericRepository.GetAllAsync();
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
