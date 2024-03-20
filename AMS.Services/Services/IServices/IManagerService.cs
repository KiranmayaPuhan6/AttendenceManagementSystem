using AMS.DtoLibrary.DTO.ManagerDto;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services.IServices
{
    public interface IManagerService
    {
        Task<ResponseList<ManagerDto>> ReadAllManagerAsync();
    }
}
