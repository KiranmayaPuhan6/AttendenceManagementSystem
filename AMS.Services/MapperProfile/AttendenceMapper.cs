using AMS.DtoLibrary.DTO.AttendenceDto;
using AMS.Entities.Models.Domain.Entities;
using AutoMapper;

namespace AMS.Services.MapperProfile
{
    public class AttendenceMapper : Profile
    {
        public AttendenceMapper()
        {
            CreateMap<Attendence,AttendenceDto>().ReverseMap();
            CreateMap<AttendenceBaseDto, Attendence>().ReverseMap();
        }
    }
}
