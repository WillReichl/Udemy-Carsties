using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        _controller = new AuctionsController(_auctionRepo, _mapper, _publishEndpoint)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    [Fact]
    public async Task GetAllAuctions_WithNoParams_Returns10Auctions()
    {
        // arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.GetAuctionsAsync(default).ReturnsForAnyArgs(auctions);

        // act
        var result = await _controller.GetAllAuctions(null);

        // assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WhenFound_ReturnsAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.GetAuctionDtoByIdAsync(default).ReturnsForAnyArgs(auction);

        // act
        var auctionId = Guid.NewGuid();
        var result = await _controller.GetAuctionById(auctionId);

        // assert
        Assert.Equal(result.Value, auction);
        await _auctionRepo.Received().GetAuctionDtoByIdAsync(Arg.Is<Guid>(a => a == auctionId));
    }

    [Fact]
    public async Task GetAuctionById_WhenNotFound_ReturnsNotFound()
    {
        // arrange
        _auctionRepo.GetAuctionDtoByIdAsync(default).ReturnsForAnyArgs(null as AuctionDto);

        // act
        var auctionId = Guid.NewGuid();
        var result = await _controller.GetAuctionById(auctionId);

        // assert
        await _auctionRepo.Received().GetAuctionDtoByIdAsync(Arg.Is<Guid>(a => a == auctionId));
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
    {
        var auctionDto = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.SaveChangesAsync().Returns(true);

        var result = await _controller.CreateAuction(auctionDto);
        var createdResult = result.Result as CreatedAtActionResult;

        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }
}
