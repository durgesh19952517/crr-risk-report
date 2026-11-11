import type { Filters, FilterOptions, Position, SummaryResult } from './types';

export function buildQuery(params: Record<string, string | number | undefined>): string {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') query.set(key, String(value));
  }
  return query.toString();
}

async function getJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  const res = await fetch(url, { signal });
  if (!res.ok) throw new Error(`Request failed (${res.status})`);
  return res.json();
}

export function fetchPositions(
  filters: Filters,
  startRow: number,
  endRow: number,
  sortBy?: string,
  sortDir?: string,
): Promise<{ rows: Position[]; total: number }> {
  const query = buildQuery({ ...filters, startRow, endRow, sortBy, sortDir });
  return getJson(`/api/positions?${query}`);
}

export function fetchSummary(
  filters: Filters,
  groupBy: string,
  signal?: AbortSignal,
): Promise<SummaryResult> {
  const query = buildQuery({ ...filters, groupBy });
  return getJson(`/api/positions/summary?${query}`, signal);
}

export function fetchFilterOptions(): Promise<FilterOptions> {
  return getJson('/api/positions/filters');
}
