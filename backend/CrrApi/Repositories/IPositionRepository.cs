using CrrApi.Models;

namespace CrrApi.Repositories;

public interface IPositionRepository
{
    PagedResult Query(PositionFilter filter, int skip, int take, string sortBy, bool descending);
    SummaryResult Summarize(PositionFilter filter, string groupBy);
    Position? GetById(string tradeId);
    FilterOptions GetFilterOptions();
}
