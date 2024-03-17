using AMS.DtoLibrary.DTO.LeaveDto;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services.IServices
{
    public interface ILeaveService
    {
        Task<Response<LeaveBaseDto>> ApplyLeaveAsync(LeaveCreationDto leaveCreationDto);
        Task<Response<LeaveDto>> ApproveLeaveAsync(LeaveUpdateDto leaveUpdateDto);
        Task<ResponseList<LeaveDto>> GetAllLeavesAsync();
        Task<Response<LeaveDto>> DeleteLeaveAsync(int id);
        Task<bool> ApplyLeavesForHolidaysAsync();
    }
}
