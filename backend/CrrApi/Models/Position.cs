using System.Text.Json.Serialization;

namespace CrrApi.Models;

public class Position
{
    public required string TradeId { get; init; }
    public required string Book { get; init; }
    public required string Commodity { get; init; }
    public required string Counterparty { get; init; }
    public required string Region { get; init; }
    public required string InstrumentType { get; init; }

    [JsonPropertyName("position")]
    public decimal Quantity { get; init; }

    public required string Currency { get; init; }
    public decimal MarketPrice { get; init; }
    public decimal Pnl { get; init; }
    public decimal FxExposure { get; init; }
    public DateOnly AsOfDate { get; init; }
}
