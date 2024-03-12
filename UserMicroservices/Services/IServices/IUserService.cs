using UserMicroservices.Models.DTO;
using UserMicroservices.Utility.ResponseModel;

namespace UserMicroservices.Services.IServices
{
    public interface IUserService
    {
        Task<Response<UserDto>> CreateNewUserAsync(UserCreationDto userCreationDto);
        Task<ResponseList<UserDto>> ReadAllUserAsync();
        Task<Response<UserDto>> DeleteUserAsync(int id);
    }
}
