using AMS.DtoLibrary.DTO.LeaveDto;
using AMS.Entities.Models.Domain.Entities;
using AutoMapper;

namespace AMS.Services.MapperProfile
{
    public class LeaveMapper : Profile
    {
        public LeaveMapper()
        {
            CreateMap<Leave, LeaveDto>().ReverseMap();
            CreateMap<LeaveCreationDto, Leave>();
            CreateMap<LeaveBaseDto, Leave>().ReverseMap();
        }
    }
}
