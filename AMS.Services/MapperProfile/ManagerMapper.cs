using AMS.DtoLibrary.DTO.ManagerDto;
using AMS.Entities.Models.Domain.Entities;
using AutoMapper;

namespace AMS.Services.MapperProfile
{
    public class ManagerMapper : Profile
    {
        public ManagerMapper()
        {
            CreateMap<ManagerCreationDto, Manager>();
            CreateMap<ManagerDto, Manager>().ReverseMap();
        }
    }
}
