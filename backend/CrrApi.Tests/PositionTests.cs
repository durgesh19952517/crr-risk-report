using CrrApi.Models;
using CrrApi.Repositories;
using CrrApi.Services;
using Xunit;

namespace CrrApi.Tests;

public class PositionTests
{
    private static Position P(string id, string book, string commodity, string counterparty,
        string region, decimal qty, decimal pnl, decimal fx) => new()
    {
        TradeId = id,
        Book = book,
        Commodity = commodity,
        Counterparty = counterparty,
        Region = region,
        InstrumentType = "Swap",
        Quantity = qty,
        Currency = "USD",
        MarketPrice = 100,
        Pnl = pnl,
        FxExposure = fx,
        AsOfDate = new DateOnly(2026, 6, 1)
    };

    private static InMemoryPositionRepository SampleRepo() => new(new[]
    {
        P("T1", "Ferrous", "HRC Steel", "Delphi Alloys", "APAC", 10, 100, 1000),
        P("T2", "Ferrous", "Iron Ore", "Cobalt Ridge", "EMEA", -20, -50, 2000),
        P("T3", "Ags", "Corn", "Vanguard Ags", "AMER", 30, 300, -500),
        P("T4", "Ags", "Corn", "Delphi Alloys", "APAC", 40, -400, -1500),
        P("T5", "Freight", "Capesize", "Pacific Bulk Carriers", "EMEA", 5, 1000, 0),
        P("T6", "Ags", "Wheat", "Vanguard Ags", "AMER", 60, 20, 250),
    });

    private static PositionService Service() => new(SampleRepo());

    [Fact]
    public void Filtering_ReturnsTotalForAllMatchingRows_NotJustThePage()
    {
        var page = Service().GetPage(new PositionQuery { Book = "Ags", StartRow = 0, EndRow = 1 });

        Assert.Equal(1, page.Rows.Count);
        Assert.Equal(3, page.Total);
    }

    [Fact]
    public void Sorting_Descending_PutsLargestFirst()
    {
        var page = Service().GetPage(new PositionQuery { SortBy = "pnl", SortDir = "desc" });

        Assert.Equal("T5", page.Rows[0].TradeId);
        Assert.Equal("T4", page.Rows[^1].TradeId);
    }

    [Fact]
    public void Paging_ReturnsTheRequestedSlice()
    {
        var page = Service().GetPage(new PositionQuery { StartRow = 2, EndRow = 4 });

        Assert.Equal(2, page.Rows.Count);
        Assert.Equal("T3", page.Rows[0].TradeId);
        Assert.Equal(6, page.Total);
    }

    [Fact]
    public void Search_MatchesCounterpartyAndCommodity()
    {
        var byCounterparty = Service().GetPage(new PositionQuery { Search = "delphi" });
        var byBoth = Service().GetPage(new PositionQuery { Search = "delphi corn" });

        Assert.Equal(2, byCounterparty.Total);
        Assert.Equal(1, byBoth.Total);
    }

    [Fact]
    public void Summary_TotalsOnlyCoverRowsMatchingTheFilter()
    {
        var summary = Service().GetSummary(new PositionFilter { Region = "APAC" }, "book");

        Assert.Equal(2, summary.Groups.Count);
        Assert.Equal(2, summary.Totals.Count);
        Assert.Equal(-300m, summary.Totals.Pnl);
        Assert.Equal(-500m, summary.Totals.FxExposure);
    }

    [Fact]
    public void Summary_OnlySumsPositionWhenGroupedByCommodity()
    {
        var byBook = Service().GetSummary(new PositionFilter(), "book");
        var byCommodity = Service().GetSummary(new PositionFilter(), "commodity");

        Assert.Null(byBook.Groups[0].Position);
        Assert.Equal(70m, byCommodity.Groups.Single(g => g.Key == "Corn").Position);
    }

    [Fact]
    public void UnknownSortField_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Service().GetPage(new PositionQuery { SortBy = "nope" }));
    }

    [Fact]
    public void UnknownGroupBy_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Service().GetSummary(new PositionFilter(), "nope"));
    }

    [Fact]
    public void PageSize_IsCapped()
    {
        var repo = new InMemoryPositionRepository(
            Enumerable.Range(1, 800).Select(i => P($"T{i}", "Ags", "Corn", "X", "APAC", 1, 1, 1)));

        var page = new PositionService(repo).GetPage(new PositionQuery { StartRow = 0, EndRow = 10_000 });

        Assert.Equal(500, page.Rows.Count);
        Assert.Equal(800, page.Total);
    }

    [Fact]
    public void CsvLoader_ParsesRows()
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, new[]
        {
            "tradeId,book,commodity,counterparty,region,instrumentType,position,currency,marketPrice,pnl,fxExposure,asOfDate",
            "TRD-000001,Ags,Palm Oil,Cobalt Ridge,APAC,Swap,-2503.8,CNY,779.79,-23003.24,-242513.25,2026-07-07"
        });

        var rows = CsvPositionLoader.Load(path);
        File.Delete(path);

        Assert.Equal(1, rows.Count);
        Assert.Equal(-2503.8m, rows[0].Quantity);
        Assert.Equal(new DateOnly(2026, 7, 7), rows[0].AsOfDate);
    }
}
