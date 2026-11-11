using CrrApi.Models;
using CrrApi.Repositories;

namespace CrrApi.Services;

public class PositionService(IPositionRepository repository)
{
    private const int MaxPageSize = 500;

    public PagedResult GetPage(PositionQuery query)
    {
        var skip = Math.Max(query.StartRow, 0);
        var take = Math.Clamp(query.EndRow - skip, 1, MaxPageSize);

        var sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "tradeId" : query.SortBy;
        if (!Fields.Sortable.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Can't sort by '{sortBy}'.");

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return repository.Query(query, skip, take, sortBy, descending);
    }

    public SummaryResult GetSummary(PositionFilter filter, string groupBy)
    {
        if (!Fields.Groupable.Contains(groupBy, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"Can't group by '{groupBy}'. Use one of: {string.Join(", ", Fields.Groupable)}.");

        return repository.Summarize(filter, groupBy);
    }

    public Position? GetPosition(string tradeId) => repository.GetById(tradeId);

    public FilterOptions GetFilterOptions() => repository.GetFilterOptions();
}
