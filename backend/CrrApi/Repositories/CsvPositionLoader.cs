using System.Globalization;
using CrrApi.Models;

namespace CrrApi.Repositories;

public static class CsvPositionLoader
{
    public static List<Position> Load(string path)
    {
        var positions = new List<Position>();
        var lineNo = 1;

        foreach (var line in File.ReadLines(path).Skip(1))
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var c = line.Split(',');
            if (c.Length != 12)
                throw new FormatException($"Line {lineNo}: expected 12 columns, got {c.Length}.");

            positions.Add(new Position
            {
                TradeId = c[0],
                Book = c[1],
                Commodity = c[2],
                Counterparty = c[3],
                Region = c[4],
                InstrumentType = c[5],
                Quantity = Num(c[6]),
                Currency = c[7],
                MarketPrice = Num(c[8]),
                Pnl = Num(c[9]),
                FxExposure = Num(c[10]),
                AsOfDate = DateOnly.ParseExact(c[11], "yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
        }

        return positions;
    }

    private static decimal Num(string s) => decimal.Parse(s, CultureInfo.InvariantCulture);
}
