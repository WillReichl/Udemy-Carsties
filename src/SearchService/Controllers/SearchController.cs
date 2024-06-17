using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        // Sorting
        query = searchParams.OrderBy switch
        {
            "make"
                => query
                    .Sort(item => item.Ascending(item2 => item2.Make))
                    .Sort(x => x.Ascending(a => a.Model)),
            "new" => query.Sort(item => item.Descending(item2 => item2.CreatedAt)),
            _ => query.Sort(item => item.Ascending(item2 => item2.AuctionEnd)),
        };

        // Filtering
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(item => item.AuctionEnd < DateTime.UtcNow),
            "endingSoon"
                => query.Match(item =>
                    item.AuctionEnd < DateTime.UtcNow.AddHours(6)
                    && item.AuctionEnd > DateTime.UtcNow
                ),
            _ => query.Match(item => item.AuctionEnd > DateTime.UtcNow),
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(item => item.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(item => item.Winner == searchParams.Winner);
        }

        // Paging
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        // Execute
        var result = await query.ExecuteAsync();

        return Ok(
            new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            }
        );
    }
}
