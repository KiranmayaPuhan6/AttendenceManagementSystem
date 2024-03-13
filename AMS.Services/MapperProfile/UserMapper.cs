using AMS.DtoLibrary.DTO.UserDto;
using AMS.Entities.Models.Domain.Entities;
using AutoMapper;

namespace AMS.Services.MapperProfile
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserCreationDto, User>();
            CreateMap<User, UserUpdateDto>().ReverseMap();
            CreateMap<UserBaseDto, User>().ReverseMap();
        }
    }
}
