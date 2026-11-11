using CrrApi.Models;
using CrrApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrrApi.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController(PositionService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult> Get([FromQuery] PositionQuery query)
    {
        try
        {
            return service.GetPage(query);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("summary")]
    public ActionResult<SummaryResult> Summary([FromQuery] PositionFilter filter, [FromQuery] string groupBy = "book")
    {
        try
        {
            return service.GetSummary(filter, groupBy);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("filters")]
    public FilterOptions Filters() => service.GetFilterOptions();

    [HttpGet("{tradeId}")]
    public ActionResult<Position> GetOne(string tradeId)
    {
        var position = service.GetPosition(tradeId);
        return position is null ? NotFound() : position;
    }
}
