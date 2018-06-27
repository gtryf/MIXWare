using System;
using System.Text;
using AutoMapper;
using MIXUI.Dtos;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ErrorInfo, ErrorInfoDto>();
            CreateMap<Submission, SuccessfulSubmissionDto>();
            CreateMap<Submission, FailedSubmissionDto>();

            CreateMap<RegisterDto, AppUser>();
            CreateMap<AppUser, UserDto>();

            CreateMap<Workspace, ShortWorkspaceDto>();
            CreateMap<CreateWorkspaceDto, Workspace>();
            CreateMap<UpdateWorkspaceDto, Workspace>();

            CreateMap<CreateFileDto, File>()
                .ForMember(dest => dest.Data, opt =>
                    opt.ResolveUsing(input =>
                        Encoding.UTF8.GetBytes(input.FileContents)));
			CreateMap<File, CreatedFileDto>();
			CreateMap<File, FileDto>()
				.ForMember(dest => dest.Data, opt => 
                    opt.ResolveUsing(input => 
                        input.Type == FileType.CompiledOutput ? Convert.ToBase64String(input.Data) : Encoding.UTF8.GetString(input.Data)));

            CreateMap<Workspace, ShortWorkspaceDto>()
                .ForMember(dest => dest.FileCount, opt => opt.ResolveUsing(w => w.Files?.Count ?? 0));
            CreateMap<Workspace, FullWorkspaceDto>()
                .ForMember(dest => dest.FileCount, opt => opt.ResolveUsing(w => w.Files?.Count ?? 0));
        }
    }
}
