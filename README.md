# CRR Risk Report

A risk-reporting view over 50,000 synthetic risk positions. The backend is a .NET 10 Web API and the frontend is React + TypeScript with AG Grid Community.

## What it does

- Scrolls through all 50,000 positions with server-side paging, sorting and filtering (AG Grid Infinite Row Model).
- Filters by book, commodity, region, instrument type and currency, plus a text search across counterparty, commodity and trade id.
- A summary panel shows P&L, FX exposure and trade count grouped by book, commodity, region or instrument. Totals always cover every row matching the current filters, not just the visible page.
- Clicking a summary row filters the grid down to that group. Clicking a grid row opens its details.
- Column order, widths and sort are remembered between visits.

## Requirements

- .NET 10 SDK
- Node.js 22 or newer

## Run it

Start the API (listens on http://localhost:5080):

```bash
dotnet run --project backend/CrrApi
```

In a second terminal, start the frontend (http://localhost:5173):

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173. The Vite dev server proxies `/api` to the API on port 5080.

## Data

`data/risk_positions.csv` has the 50,000 rows and is read once when the API starts, so there is no seeding step. The path can be changed with `DataPath` in `backend/CrrApi/appsettings.json`.

## Tests

```bash
dotnet test backend/CrrApi.Tests
cd frontend && npm test
```

GitHub Actions (`.github/workflows/ci.yml`) runs both and builds the frontend.

## API

| Endpoint | Purpose |
|---|---|
| `GET /api/positions` | One page of rows plus the total matching rows |
| `GET /api/positions/summary` | Totals per group, plus an overall total |
| `GET /api/positions/filters` | Distinct values for the filter dropdowns |
| `GET /api/positions/{tradeId}` | A single position |

Filters (all optional): `search`, `book`, `commodity`, `region`, `instrumentType`, `currency`.

`/api/positions` also takes `startRow`, `endRow` (max 500 rows per page), `sortBy` and `sortDir` (`asc` or `desc`). `/api/positions/summary` takes `groupBy`: `book`, `commodity`, `region` or `instrumentType`.

Example:

```
GET /api/positions?book=Ags&search=palm&sortBy=pnl&sortDir=desc&startRow=0&endRow=100
GET /api/positions/summary?groupBy=commodity&book=Ags
```

## Layout

```
backend/CrrApi         controllers -> service -> repository
backend/CrrApi.Tests   xUnit tests
frontend/src           React app (components, api client)
data/                  risk_positions.csv
```

See [DESIGN.md](DESIGN.md) for the design decisions.
