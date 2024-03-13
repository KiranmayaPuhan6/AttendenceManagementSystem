using AutoMapper;
using UserMicroservices.Models.Domain.Entities;
using UserMicroservices.Models.DTO;

namespace UserMicroservices.Mapper
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
