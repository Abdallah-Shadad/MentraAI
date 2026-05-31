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
            .ForMember(d => d.Age, o => o.MapFrom(s => s.Profile.Age))
            .ForMember(d => d.EdLevel, o => o.MapFrom(s => s.Profile.EdLevel))
            .ForMember(d => d.YearsCode, o => o.MapFrom(s => s.Profile.YearsCode))
            .ForMember(d => d.WorkExp, o => o.MapFrom(s => s.Profile.WorkExp))
            .ForMember(d => d.Employment, o => o.MapFrom(s => s.Profile.Employment))
            .ForMember(d => d.RemoteWork, o => o.MapFrom(s => s.Profile.RemoteWork))
            .ForMember(d => d.Industry, o => o.MapFrom(s => s.Profile.Industry))
            .ForMember(d => d.OrgSize, o => o.MapFrom(s => s.Profile.OrgSize))
            .ForMember(d => d.AISelect, o => o.MapFrom(s => s.Profile.AISelect))
            .ForMember(d => d.IsOnboarded, o => o.MapFrom(s => s.Profile.IsOnboarded))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.Profile.CreatedAt))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.Profile.UpdatedAt))
            .ForMember(d => d.CurrentSkills, o => o.MapFrom(s => DeserializeList(s.Profile.CurrentSkillsJson)))
            .ForMember(d => d.FutureSkills, o => o.MapFrom(s => DeserializeList(s.Profile.FutureSkillsJson)));
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch { return new List<string>(); }
    }
}