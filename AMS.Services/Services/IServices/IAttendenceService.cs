using AMS.DtoLibrary.DTO.AttendenceDto;
using AMS.DtoLibrary.DTO.UserDto;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services.IServices
{
    public interface IAttendenceService
    {
        Task<Response<AttendenceBaseDto>> AttendenceLogIn(int userId);
        Task<Response<AttendenceBaseDto>> AttendenceLogOut(int userId);
        Task<ResponseList<AttendenceDto>> GetAllAttendenceAsync();
        Task<Response<AttendenceDto>> DeleteAttendenceAsync(int id);
    }
}
