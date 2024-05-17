using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using NSubstitute;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly IAuctionRepository _auctionRepo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = Substitute.For<IAuctionRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);

        _controller = new AuctionsController(_auctionRepo, _mapper, _publishEndpoint);
    }
}
