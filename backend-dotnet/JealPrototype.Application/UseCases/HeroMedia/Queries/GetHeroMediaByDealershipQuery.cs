using MediatR;
using JealPrototype.Application.DTOs.HeroMedia;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.HeroMedia.Queries;

public record GetHeroMediaByDealershipQuery(int DealershipId) : IRequest<HeroMediaDto?>;

public class GetHeroMediaByDealershipQueryHandler : IRequestHandler<GetHeroMediaByDealershipQuery, HeroMediaDto?>
{
    private readonly IHeroMediaRepository _heroMediaRepository;

    public GetHeroMediaByDealershipQueryHandler(IHeroMediaRepository heroMediaRepository)
    {
        _heroMediaRepository = heroMediaRepository;
    }

    public async Task<HeroMediaDto?> Handle(GetHeroMediaByDealershipQuery request, CancellationToken cancellationToken)
    {
        var heroMedia = await _heroMediaRepository.GetByDealershipIdAsync(request.DealershipId);
        if (heroMedia == null)
            return null;

        return new HeroMediaDto
        {
            Id = heroMedia.Id,
            DealershipId = heroMedia.DealershipId,
            MediaType = heroMedia.MediaType,
            MediaUrl = heroMedia.MediaUrl,
            CreatedAt = heroMedia.CreatedAt,
            UpdatedAt = heroMedia.UpdatedAt
        };
    }
}
