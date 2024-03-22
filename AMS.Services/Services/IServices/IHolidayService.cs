using AMS.DtoLibrary.DTO.HolidayDto;
using AMS.Entities.Models.Domain.Entities;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services.IServices
{
    public interface IHolidayService
    {
        Task<Response<HolidayCreationDto>> CreateNewHolidayAsync(HolidayCreationDto holidayCreationDto);
        Task<ResponseList<Holidays>> GetAllHolidaysAsync();
        Task<bool> DeleteAllHolidaysAsync(string year);
    }
}
