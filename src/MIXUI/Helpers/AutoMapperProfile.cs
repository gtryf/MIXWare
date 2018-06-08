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
            CreateMap<CreateWorkspaceDto, Workspace>();

            CreateMap<Folder, StorableDto>()
                .ForMember(dest => dest.Type, opts => opts.UseValue("folder"));
            CreateMap<File, StorableDto>()
                .ForMember(dest => dest.Type, opts => opts.UseValue("file"));
        }
    }
}
