namespace CrrApi.Models;

public record PagedResult(IReadOnlyList<Position> Rows, int Total);

// Position is only filled in when grouping by commodity, because quantities
// in different commodities use different units and shouldn't be added up.
public record GroupSummary(string Key, int Count, decimal Pnl, decimal FxExposure, decimal? Position);

public record SummaryResult(IReadOnlyList<GroupSummary> Groups, GroupSummary Totals);

public record FilterOptions(
    IReadOnlyList<string> Books,
    IReadOnlyList<string> Commodities,
    IReadOnlyList<string> Regions,
    IReadOnlyList<string> InstrumentTypes,
    IReadOnlyList<string> Currencies);
