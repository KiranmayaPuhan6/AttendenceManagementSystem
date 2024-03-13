using UserMicroservices.Models.DTO;
using UserMicroservices.Utility.ResponseModel;

namespace UserMicroservices.Services.IServices
{
    public interface IUserService
    {
        Task<Response<UserBaseDto>> CreateNewUserAsync(UserCreationDto userCreationDto);
        Task<ResponseList<UserDto>> ReadAllUserAsync();
        Task<Response<UserBaseDto>> UpdateUserAsync(UserUpdateDto userUpdateDto);
        Task<Response<UserDto>> DeleteUserAsync(int id);
        Task<Response<UserDto>> ReadUserAsync(int id);
    }
}
