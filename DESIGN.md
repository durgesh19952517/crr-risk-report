# Design notes

## Overview

A .NET 10 API serves the positions and a React + AG Grid frontend displays them. The backend is split into controller, service and repository. The controller handles HTTP, the service validates input (page size, sort and group fields), and the repository does the actual querying behind an `IPositionRepository` interface.

## Handling the large dataset

- **Server-side everything.** The grid uses the Infinite Row Model and asks the API for 100 rows at a time. Filtering, sorting and paging all happen on the server, so the browser never holds more than a few hundred rows.
- **Totals come from the API.** The summary endpoint takes the same filters as the grid and aggregates over every matching row. The grid and the summary panel always agree because they share one filter object.
- **Position is not summed across commodities.** Quantities are in different units (a Capesize freight position and a tonne of palm oil are not comparable), so the summary only shows a position total when grouped by commodity. For other groupings only P&L and FX exposure are totalled.
- **Money uses `decimal`** in the API so sums don't pick up floating point noise.
- **Search** is debounced (300ms) in the UI. On the server every word must appear in the counterparty, commodity or trade id.

## Data store tradeoff

I used an in-memory store loaded from the CSV at startup. I chose it to keep the project easy to run (no database to set up) and because 50,000 rows is small: a page, sort or summary request takes roughly 15ms on my machine.

The downsides are that data is lost on restart and never changes while running, every request filters and sorts the whole list (O(n log n) for sorting), and it would not scale to tens of millions of rows or several API instances. Because the repository sits behind an interface, swapping the store means writing one new class.

## How I would use OpenSearch

- **Index:** one document per position. `book`, `region`, `instrumentType`, `currency` and `commodity` as `keyword` fields for exact filtering and aggregations; `counterparty` and `commodity` also as `text` (via a multi-field) for search; `position`, `marketPrice`, `pnl` and `fxExposure` as `double` (or `scaled_float`); `asOfDate` as `date`.
- **Table queries:** a `bool` query with `term` filters for the dropdowns and a `multi_match` with `fuzziness: AUTO` over counterparty and commodity for the search box. Sorting uses the field's doc values. Paging uses `from`/`size` for the first few thousand rows, and `search_after` with `tradeId` as a tie-breaker for deep scrolling.
- **Summary:** the same filter wrapped in a `terms` aggregation on the group field with `sum` sub-aggregations for P&L, FX exposure and position, and `size: 0` so no documents are returned.
- **Loading:** a bulk-index job that reads the CSV, with the index rebuilt behind an alias.

## What I'd improve with more time

- Add per-column AG Grid filters (numeric ranges, date ranges) instead of only the filter bar. The API would take a filter model.
- A P&L-by-book/region chart next to the summary.
- Saved views (named filters + column layout) stored on the server rather than in localStorage.
- Move to Postgres or OpenSearch and add integration tests against it, plus API-level tests with `WebApplicationFactory`.
- Better handling of errors and loading states in the UI (retry button, skeleton rows).
- Response compression and caching of the summary for unfiltered requests.
