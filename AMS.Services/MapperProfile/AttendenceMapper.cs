using AMS.DtoLibrary.DTO.AttendenceDto;
using AMS.Entities.Models.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
