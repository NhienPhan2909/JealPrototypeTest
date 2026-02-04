using AutoMapper;
using JealPrototype.Application.DTOs.Auth;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Application.DTOs.User;
using JealPrototype.Application.DTOs.Lead;
using JealPrototype.Application.DTOs.SalesRequest;
using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.ValueObjects;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => ConvertUserTypeToString(src.UserType)))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(p => ConvertPermissionToString(p)).ToList()));

        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => ConvertUserTypeToString(src.UserType)))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(p => ConvertPermissionToString(p)).ToList()));

        // Dealership mappings
        CreateMap<Dealership, DealershipResponseDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.ThemeColor, opt => opt.MapFrom(src => src.ThemeColor.Value))
            .ForMember(dest => dest.SecondaryThemeColor, opt => opt.MapFrom(src => src.SecondaryThemeColor.Value))
            .ForMember(dest => dest.BodyBackgroundColor, opt => opt.MapFrom(src => src.BodyBackgroundColor.Value))
            .ForMember(dest => dest.HeroType, opt => opt.MapFrom(src => src.HeroType.ToString().ToLower()))
            .ForMember(dest => dest.NavigationConfig, opt => opt.MapFrom(src => src.NavigationConfigJson));

        // Vehicle mappings
        CreateMap<Vehicle, VehicleResponseDto>()
            .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.ToString().ToLower()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLower()));

        // Lead mappings
        CreateMap<Lead, LeadResponseDto>()
            .ForMember(dest => dest.VehicleTitle, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.Title : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status == LeadStatus.InProgress ? "in progress" : src.Status.ToString().ToLower()));

        // SalesRequest mappings
        CreateMap<SalesRequest, SalesRequestResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status == LeadStatus.InProgress ? "in progress" : src.Status.ToString().ToLower()));

        // BlogPost mappings
        CreateMap<BlogPost, BlogPostResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLower()));
    }

    private static string ConvertUserTypeToString(UserType userType)
    {
        return userType switch
        {
            UserType.Admin => "admin",
            UserType.DealershipOwner => "dealership_owner",
            UserType.DealershipStaff => "dealership_staff",
            _ => userType.ToString().ToLower()
        };
    }

    private static string ConvertPermissionToString(Permission permission)
    {
        return permission switch
        {
            Permission.Leads => "leads",
            Permission.SalesRequests => "sales_requests",
            Permission.Vehicles => "vehicles",
            Permission.Blogs => "blogs",
            Permission.Settings => "settings",
            _ => permission.ToString().ToLower()
        };
    }
}
