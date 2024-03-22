using AMS.DtoLibrary.DTO.HolidayDto;
using AMS.Entities.Models.Domain.Entities;
using AutoMapper;

namespace AMS.Services.MapperProfile
{
    public class HolidayMapper : Profile
    {
        public HolidayMapper()
        {
            CreateMap<HolidayCreationDto, Holidays>();
        }
    }
}
