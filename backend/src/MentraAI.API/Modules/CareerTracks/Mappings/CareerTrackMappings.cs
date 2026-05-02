using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using MentraAI.API.Modules.CareerTracks.DTOs.Responses;
using MentraAI.API.Modules.CareerTracks.Models;

namespace MentraAI.API.Modules.CareerTracks.Mappings;

public class CareerTrackMappingProfile : Profile
{
    public CareerTrackMappingProfile()
    {
        // CareerTrack -> CareerTrackResponse
        CreateMap<CareerTrack, CareerTrackResponse>()
            .ForMember(d => d.CareerTrackId, o => o.MapFrom(s => s.Id));

        // UserTrack -> SelectTrackResponse
        CreateMap<UserTrack, SelectTrackResponse>()
            .ForMember(d => d.UserTrackId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CareerTrackId, o => o.MapFrom(s => s.CareerTrackId))
            .ForMember(d => d.CareerTrackName, o => o.MapFrom(s => s.CareerTrack.Name));

        // UserTrack -> MyTrackResponse
        // HasRoadmap is not on the model — service injects it after mapping
        CreateMap<UserTrack, MyTrackResponse>()
            .ForMember(d => d.UserTrackId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CareerTrackId, o => o.MapFrom(s => s.CareerTrackId))
            .ForMember(d => d.CareerTrackName, o => o.MapFrom(s => s.CareerTrack.Name))
            .ForMember(d => d.Slug, o => o.MapFrom(s => s.CareerTrack.Slug))
            .ForMember(d => d.HasRoadmap, o => o.Ignore());
    }
}

// Static helper — used by service to deserialize TopRolesJson without depending on AutoMapper
public static class PredictionMapper
{
    public static PredictionResponse ToPredictionResponse(MLPrediction prediction)
    {
        List<RoleItem> topRoles;

        try
        {
            var raw = JsonSerializer.Deserialize<List<RoleRaw>>(prediction.TopRolesJson)
                      ?? new List<RoleRaw>();

            topRoles = raw
                .Select(r => new RoleItem { Name = r.Name, Confidence = r.Confidence })
                .ToList();
        }
        catch
        {
            topRoles = new List<RoleItem>();
        }

        return new PredictionResponse
        {
            PrimaryRole = new RoleItem
            {
                Name = prediction.PrimaryRoleName,
                Confidence = prediction.Confidence
            },
            TopRoles = topRoles,
            PredictedAt = prediction.CreatedAt
        };
    }

    // Private deserialization type — matches AI response JSON shape
    private class RoleRaw
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("confidence")] public decimal Confidence { get; set; }
    }
}