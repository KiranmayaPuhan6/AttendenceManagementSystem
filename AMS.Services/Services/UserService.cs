﻿using AMS.Entities.Models.Domain.Entities;
using AMS.Services.Services.IServices;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.Extensions.Configuration;
using System.Net;
using AMS.Entities.Infrastructure.Repository.IRepository;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Services.Utility.ResponseModel;
using BCrypt.Net;
using AMS.Services.Utility;
using Microsoft.Extensions.Logging;
using AMS.Services.Utility.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace AMS.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _genericRepository;
        private readonly ICacheService _cacheService;
        private readonly IResponseService _responseService;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        public UserService(IGenericRepository<User> genericRepository, ICacheService cacheService, IResponseService responseService, 
            ILogger<UserService> logger, IMapper mapper, IWebHostEnvironment environment, IEmailService emailService)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _responseService = responseService ?? throw new ArgumentNullException(nameof(responseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
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
                EmailAddress emailAddress = new EmailAddress
                {
                    To = user.Email,
                    Subject = $"Registration Successful",
                    Message = $"<html><body><p>Hi {user.Name},</p><p>Thanks for registering to Attendence Management System.</p><p> Have a nice day.</p><p> Thanks,</p><p> AMS Team</p></body></html>",
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
            return await _genericRepository.GetAllAsync(predicate: null,includes: q => q.Include(u => u.Attendence));
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
