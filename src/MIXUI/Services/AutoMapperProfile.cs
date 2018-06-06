using AutoMapper;
using MIXUI.Dtos;
using MIXUI.Entities;

namespace MIXUI.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterDto, User>();
            CreateMap<User, UserDto>();
        }
    }
}
