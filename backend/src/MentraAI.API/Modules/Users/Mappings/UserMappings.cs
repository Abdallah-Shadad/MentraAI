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
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.User.Id))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.User.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.User.LastName))
            .ForMember(d => d.Background, o => o.MapFrom(s => s.Profile.Background))
            .ForMember(d => d.WeeklyHours, o => o.MapFrom(s => s.Profile.WeeklyHours))
            .ForMember(d => d.CareerGoals, o => o.MapFrom(s => s.Profile.CareerGoals))
            .ForMember(d => d.IsOnboarded, o => o.MapFrom(s => s.Profile.IsOnboarded))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.Profile.CreatedAt))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.Profile.UpdatedAt))
            .ForMember(d => d.CurrentSkills, o => o.MapFrom(s => DeserializeList(s.Profile.CurrentSkillsJson)))
            .ForMember(d => d.Interests, o => o.MapFrom(s => DeserializeList(s.Profile.InterestsJson)));
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch { return new List<string>(); }
    }
}