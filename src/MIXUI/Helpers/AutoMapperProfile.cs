using AutoMapper;
using MIXUI.Dtos;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterDto, AppUser>();
            CreateMap<AppUser, UserDto>();

            CreateMap<Workspace, WorkspaceDto>();
        }
    }
}
