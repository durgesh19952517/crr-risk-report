namespace CrrApi.Models;

public class PositionFilter
{
    public string? Search { get; set; }
    public string? Book { get; set; }
    public string? Commodity { get; set; }
    public string? Region { get; set; }
    public string? InstrumentType { get; set; }
    public string? Currency { get; set; }
}

public class PositionQuery : PositionFilter
{
    public int StartRow { get; set; } = 0;
    public int EndRow { get; set; } = 100;
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "asc";
}

public static class Fields
{
    public static readonly string[] Sortable =
    {
        "tradeId", "book", "commodity", "counterparty", "region", "instrumentType",
        "position", "currency", "marketPrice", "pnl", "fxExposure", "asOfDate"
    };

    public static readonly string[] Groupable = { "book", "commodity", "region", "instrumentType" };
}
