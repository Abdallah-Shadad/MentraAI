using AutoMapper;
using MentraAI.API.Modules.Auth.DTOs.Responses;
using MentraAI.API.Modules.Auth.Models;

namespace MentraAI.API.Modules.Auth.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<ApplicationUser, UserSummary>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsOnboarded, opt => opt.Ignore()); // set manually

        CreateMap<ApplicationUser, RegisterResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
    }
}