using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Domain.ValueObjects;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.UseCases.Dealership;

public class UpdateDealershipUseCase
{
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IMapper _mapper;

    public UpdateDealershipUseCase(IDealershipRepository dealershipRepository, IMapper mapper)
    {
        _dealershipRepository = dealershipRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DealershipResponseDto>> ExecuteAsync(
        int dealershipId,
        UpdateDealershipDto request,
        CancellationToken cancellationToken = default)
    {
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId, cancellationToken);

        if (dealership == null)
        {
            return ApiResponse<DealershipResponseDto>.ErrorResponse("Dealership not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Name) || !string.IsNullOrWhiteSpace(request.Address) || 
            !string.IsNullOrWhiteSpace(request.Phone) || !string.IsNullOrWhiteSpace(request.Email) ||
            request.LogoUrl != null || request.Hours != null || request.About != null || request.WebsiteUrl != null)
        {
            dealership.Update(
                request.Name ?? dealership.Name,
                request.Address ?? dealership.Address,
                request.Phone ?? dealership.Phone,
                request.Email ?? dealership.Email.Value,
                request.LogoUrl ?? dealership.LogoUrl,
                request.Hours ?? dealership.Hours,
                request.About ?? dealership.About,
                request.WebsiteUrl ?? dealership.WebsiteUrl);
        }

        if (request.FinancePolicy != null)
            dealership.UpdateFinancePolicy(request.FinancePolicy);

        if (request.WarrantyPolicy != null)
            dealership.UpdateWarrantyPolicy(request.WarrantyPolicy);

        if (!string.IsNullOrWhiteSpace(request.HeroType) || request.HeroBackgroundImage != null || request.HeroVideoUrl != null || request.HeroCarouselImages != null || request.HeroTitle != null || request.HeroSubtitle != null)
        {
            var heroType = !string.IsNullOrWhiteSpace(request.HeroType)
                ? request.HeroType.ToLower() switch
                {
                    "image" => HeroType.Image,
                    "video" => HeroType.Video,
                    "carousel" => HeroType.Carousel,
                    _ => dealership.HeroType
                }
                : dealership.HeroType;
            
            dealership.UpdateHeroSettings(
                heroType, 
                request.HeroBackgroundImage, 
                request.HeroVideoUrl, 
                request.HeroCarouselImages,
                request.HeroTitle,
                request.HeroSubtitle);
        }

        if (!string.IsNullOrWhiteSpace(request.ThemeColor) || !string.IsNullOrWhiteSpace(request.SecondaryThemeColor) || 
            !string.IsNullOrWhiteSpace(request.BodyBackgroundColor) || !string.IsNullOrWhiteSpace(request.FontFamily))
        {
            dealership.UpdateBranding(
                request.ThemeColor ?? dealership.ThemeColor.Value,
                request.SecondaryThemeColor ?? dealership.SecondaryThemeColor.Value,
                request.BodyBackgroundColor ?? dealership.BodyBackgroundColor.Value,
                request.FontFamily ?? dealership.FontFamily);
        }

        if (request.NavigationConfig != null)
            dealership.UpdateNavigationConfig(request.NavigationConfig);

        if (request.FacebookUrl != null || request.InstagramUrl != null)
            dealership.UpdateSocialMedia(request.FacebookUrl, request.InstagramUrl);

        if (request.FinancePromoImage != null || request.FinancePromoText != null)
            dealership.UpdateFinancePromo(request.FinancePromoImage, request.FinancePromoText);

        if (request.WarrantyPromoImage != null || request.WarrantyPromoText != null)
            dealership.UpdateWarrantyPromo(request.WarrantyPromoImage, request.WarrantyPromoText);

        await _dealershipRepository.UpdateAsync(dealership, cancellationToken);

        var response = _mapper.Map<DealershipResponseDto>(dealership);
        return ApiResponse<DealershipResponseDto>.SuccessResponse(response, "Dealership updated successfully");
    }
}
