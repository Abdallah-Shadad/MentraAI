using System.Text.Json;
using AutoMapper;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.Users.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<(ApplicationUser User, UserProfile Profile), UserProfileResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Background, opt => opt.MapFrom(src => src.Profile.Background))
            .ForMember(dest => dest.WeeklyHours, opt => opt.MapFrom(src => src.Profile.WeeklyHours))
            .ForMember(dest => dest.CareerGoals, opt => opt.MapFrom(src => src.Profile.CareerGoals))
            .ForMember(dest => dest.IsOnboarded, opt => opt.MapFrom(src => src.Profile.IsOnboarded))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Profile.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Profile.UpdatedAt))
            .ForMember(dest => dest.CurrentSkills, opt => opt.MapFrom(src => DeserializeList(src.Profile.CurrentSkillsJson)))
            .ForMember(dest => dest.Interests, opt => opt.MapFrom(src => DeserializeList(src.Profile.InterestsJson)));
    }

    private static List<string> DeserializeList(string? json)
    {
        if (json is null) return new List<string>();
        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }
}