using AutoMapper;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Application.Mappings;

/// <summary>
/// AutoMapper profile for EasyCars credential mappings
/// </summary>
public class EasyCarsCredentialProfile : Profile
{
    public EasyCarsCredentialProfile()
    {
        // Entity to CredentialResponse
        CreateMap<EasyCarsCredential, CredentialResponse>()
            .ForMember(dest => dest.Environment, 
                opt => opt.MapFrom(src => src.Environment.ToString()))
            .ForMember(dest => dest.LastSyncedAt, 
                opt => opt.MapFrom(src => (DateTime?)null)); // Will be populated from sync logs if needed

        // Entity to CredentialMetadataResponse
        CreateMap<EasyCarsCredential, CredentialMetadataResponse>()
            .ForMember(dest => dest.Environment, 
                opt => opt.MapFrom(src => src.Environment.ToString()))
            .ForMember(dest => dest.HasCredentials, 
                opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.ConfiguredAt, 
                opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastSyncedAt, 
                opt => opt.MapFrom(src => (DateTime?)null)); // Will be populated from sync logs if needed
    }
}
