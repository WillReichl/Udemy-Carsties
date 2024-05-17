using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAuctionRepository _auctionRepository;

    public AuctionsController(
        IAuctionRepository auctionRepository,
        IMapper mapper,
        IPublishEndpoint publishEndpoint
    )
    {
        _auctionRepository = auctionRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _auctionRepository.GetAuctionsAsync(date);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auctionDto = await _auctionRepository.GetAuctionDtoByIdAsync(id);

        if (auctionDto == null)
            return NotFound();

        return auctionDto;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        auction.Seller = User.Identity.Name;

        _auctionRepository.AddAuction(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var isSaved = await _auctionRepository.SaveChangesAsync();

        if (!isSaved)
            return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _auctionRepository.GetAuctionEntityById(id);

        if (auction == null)
            return NotFound();

        if (auction.Seller != User.Identity.Name)
            return Forbid();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var isSaved = await _auctionRepository.SaveChangesAsync();

        if (isSaved)
            return Ok();

        return BadRequest("Problem saving changes");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _auctionRepository.GetAuctionEntityById(id);

        if (auction == null)
            return NotFound();

        if (auction.Seller != User.Identity.Name)
            return Forbid();

        _auctionRepository.RemoveAuction(auction);

        await _publishEndpoint.Publish(new AuctionDeleted { Id = auction.Id.ToString() });

        var isDeleted = await _auctionRepository.SaveChangesAsync();

        if (isDeleted)
            return Ok();

        return BadRequest("Could not delete auction");
    }
}
