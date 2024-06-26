﻿using AMS.DtoLibrary.DTO.UserDto;
using AMS.Entities.Models.Domain.Entities;
using AMS.Services.Utility;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services.IServices
{
    public interface IUserService
    {
        Task<Response<UserBaseDto>> CreateNewUserAsync(UserCreationDto userCreationDto);
        Task<ResponseList<UserDto>> ReadAllUserAsync();
        Task<Response<UserBaseDto>> UpdateUserAsync(UserUpdateDto userUpdateDto);
        Task<Response<UserBaseDto>> UpdateManagerAsync(UserManagerUpdateDto userManagerUpdateDto);
        Task<string> UpdateRoleAsync(UserRoleUpdateDto role);
        Task<bool> AddRefreshTokenAsync(string email, string refreshToken);
        Task<Response<UserDto>> DeleteUserAsync(int id);
        Task<Response<UserDto>> ReadUserAsync(int id);
        Task<bool> VerifyEmailAsync(int userId, int token);
        Task<bool> VerificationCodeForEmailAsync(int userId);
        Task<bool> VerifyPhoneNumberAsync(int userId, int token);
        Task<bool> VerificationCodeForPhoneNumberAsync(int userId);
        Task<bool> ResetPasswordTokenAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPassword data);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
