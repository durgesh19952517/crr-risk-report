export interface Position {
  tradeId: string;
  book: string;
  commodity: string;
  counterparty: string;
  region: string;
  instrumentType: string;
  position: number;
  currency: string;
  marketPrice: number;
  pnl: number;
  fxExposure: number;
  asOfDate: string;
}

export interface Filters {
  search: string;
  book: string;
  commodity: string;
  region: string;
  instrumentType: string;
  currency: string;
}

export const emptyFilters: Filters = {
  search: '',
  book: '',
  commodity: '',
  region: '',
  instrumentType: '',
  currency: '',
};

export type GroupField = 'book' | 'commodity' | 'region' | 'instrumentType';

export interface GroupSummary {
  key: string;
  count: number;
  pnl: number;
  fxExposure: number;
  position: number | null;
}

export interface SummaryResult {
  groups: GroupSummary[];
  totals: GroupSummary;
}

export interface FilterOptions {
  books: string[];
  commodities: string[];
  regions: string[];
  instrumentTypes: string[];
  currencies: string[];
}
