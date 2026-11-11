using CrrApi.Models;

namespace CrrApi.Repositories;

public class InMemoryPositionRepository : IPositionRepository
{
    private static readonly Dictionary<string, Func<Position, IComparable>> SortKeys =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["tradeId"] = p => p.TradeId,
            ["book"] = p => p.Book,
            ["commodity"] = p => p.Commodity,
            ["counterparty"] = p => p.Counterparty,
            ["region"] = p => p.Region,
            ["instrumentType"] = p => p.InstrumentType,
            ["position"] = p => p.Quantity,
            ["currency"] = p => p.Currency,
            ["marketPrice"] = p => p.MarketPrice,
            ["pnl"] = p => p.Pnl,
            ["fxExposure"] = p => p.FxExposure,
            ["asOfDate"] = p => p.AsOfDate
        };

    private static readonly Dictionary<string, Func<Position, string>> GroupKeys =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["book"] = p => p.Book,
            ["commodity"] = p => p.Commodity,
            ["region"] = p => p.Region,
            ["instrumentType"] = p => p.InstrumentType
        };

    private readonly List<Position> _positions;
    private readonly Dictionary<string, Position> _byId;

    public InMemoryPositionRepository(IEnumerable<Position> positions)
    {
        _positions = positions.ToList();
        _byId = _positions.ToDictionary(p => p.TradeId);
    }

    public int Count => _positions.Count;

    public PagedResult Query(PositionFilter filter, int skip, int take, string sortBy, bool descending)
    {
        var filtered = Apply(filter).ToList();
        var key = SortKeys[sortBy];

        var ordered = descending ? filtered.OrderByDescending(key) : filtered.OrderBy(key);
        var rows = ordered
            .ThenBy(p => p.TradeId, StringComparer.Ordinal)
            .Skip(skip)
            .Take(take)
            .ToList();

        return new PagedResult(rows, filtered.Count);
    }

    public SummaryResult Summarize(PositionFilter filter, string groupBy)
    {
        var key = GroupKeys[groupBy];
        var byCommodity = groupBy.Equals("commodity", StringComparison.OrdinalIgnoreCase);

        var groups = Apply(filter)
            .GroupBy(key)
            .Select(g => new GroupSummary(
                g.Key,
                g.Count(),
                g.Sum(p => p.Pnl),
                g.Sum(p => p.FxExposure),
                byCommodity ? g.Sum(p => p.Quantity) : (decimal?)null))
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        var totals = new GroupSummary(
            "Total",
            groups.Sum(g => g.Count),
            groups.Sum(g => g.Pnl),
            groups.Sum(g => g.FxExposure),
            null);

        return new SummaryResult(groups, totals);
    }

    public Position? GetById(string tradeId) => _byId.GetValueOrDefault(tradeId);

    public FilterOptions GetFilterOptions() => new(
        Distinct(p => p.Book),
        Distinct(p => p.Commodity),
        Distinct(p => p.Region),
        Distinct(p => p.InstrumentType),
        Distinct(p => p.Currency));

    private List<string> Distinct(Func<Position, string> selector) =>
        _positions.Select(selector).Distinct().Order(StringComparer.Ordinal).ToList();

    private IEnumerable<Position> Apply(PositionFilter f)
    {
        var q = _positions.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(f.Book)) q = q.Where(p => Same(p.Book, f.Book));
        if (!string.IsNullOrWhiteSpace(f.Commodity)) q = q.Where(p => Same(p.Commodity, f.Commodity));
        if (!string.IsNullOrWhiteSpace(f.Region)) q = q.Where(p => Same(p.Region, f.Region));
        if (!string.IsNullOrWhiteSpace(f.InstrumentType)) q = q.Where(p => Same(p.InstrumentType, f.InstrumentType));
        if (!string.IsNullOrWhiteSpace(f.Currency)) q = q.Where(p => Same(p.Currency, f.Currency));

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            // every word has to match counterparty, commodity or trade id
            var words = f.Search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            q = q.Where(p => words.All(w =>
                Has(p.Counterparty, w) || Has(p.Commodity, w) || Has(p.TradeId, w)));
        }

        return q;
    }

    private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool Has(string value, string word) => value.Contains(word, StringComparison.OrdinalIgnoreCase);
}
