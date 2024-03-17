using AMS.DtoLibrary.DTO.UserDto;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services.IServices
{
    public interface IUserService
    {
        Task<Response<UserBaseDto>> CreateNewUserAsync(UserCreationDto userCreationDto);
        Task<ResponseList<UserDto>> ReadAllUserAsync();
        Task<Response<UserBaseDto>> UpdateUserAsync(UserUpdateDto userUpdateDto);
        Task<Response<UserDto>> DeleteUserAsync(int id);
        Task<Response<UserDto>> ReadUserAsync(int id);
        Task<bool> VerifyEmailAsync(int userId, int token);
        Task<bool> VerificationCodeForEmailAsync(int userId);
    }
}
