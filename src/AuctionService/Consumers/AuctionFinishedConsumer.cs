using AuctionService.Data;
using MassTransit;
using Contracts;
using AuctionService.Entities;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _auctionDbContext;

    public AuctionFinishedConsumer(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
    }

    public async Task Consume(ConsumeContext<AuctionFinished> consumeContext)
    {
        Console.WriteLine("--> Consuming auction finished");

        var auction = await _auctionDbContext.Auctions.FindAsync(consumeContext.Message.AuctionId);

        if (consumeContext.Message.ItemSold) 
        {
            auction.Winner = consumeContext.Message.Winner;
            auction.SoldAmount = consumeContext.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
            ? Status.Finished
            : Status.ReserveNotMet;

        await _auctionDbContext.SaveChangesAsync();
    }
}
