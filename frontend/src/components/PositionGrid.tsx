import { useEffect, useMemo, useState } from 'react';
import { AgGridReact } from 'ag-grid-react';
import {
  AllCommunityModule,
  ModuleRegistry,
  type ColDef,
  type GridApi,
  type GridReadyEvent,
  type IDatasource,
  type ValueFormatterParams,
} from 'ag-grid-community';
import { fetchPositions } from '../api';
import { formatDecimal, formatWhole } from '../format';
import type { Filters, Position } from '../types';

ModuleRegistry.registerModules([AllCommunityModule]);

const COLUMN_STATE_KEY = 'crr-column-state';

const signClass = (params: { value?: number }) =>
  params.value == null ? '' : params.value < 0 ? 'negative' : 'positive';

const columns: ColDef<Position>[] = [
  { field: 'tradeId', headerName: 'Trade ID', width: 120 },
  { field: 'book', width: 120 },
  { field: 'commodity', width: 130 },
  { field: 'counterparty', width: 190 },
  { field: 'region', width: 90 },
  { field: 'instrumentType', headerName: 'Instrument', width: 110 },
  {
    field: 'position',
    type: 'numericColumn',
    width: 110,
    valueFormatter: (p: ValueFormatterParams) => formatDecimal(p.value),
  },
  { field: 'currency', width: 90 },
  {
    field: 'marketPrice',
    headerName: 'Mkt Price',
    type: 'numericColumn',
    width: 110,
    valueFormatter: (p: ValueFormatterParams) => formatDecimal(p.value),
  },
  {
    field: 'pnl',
    headerName: 'P&L (USD)',
    type: 'numericColumn',
    width: 130,
    cellClass: signClass,
    valueFormatter: (p: ValueFormatterParams) => formatWhole(p.value),
  },
  {
    field: 'fxExposure',
    headerName: 'FX Exposure (USD)',
    type: 'numericColumn',
    width: 150,
    cellClass: signClass,
    valueFormatter: (p: ValueFormatterParams) => formatWhole(p.value),
  },
  { field: 'asOfDate', headerName: 'As Of', width: 110 },
];

interface Props {
  filters: Filters;
  onRowSelected: (position: Position) => void;
}

export function PositionGrid({ filters, onRowSelected }: Props) {
  const [api, setApi] = useState<GridApi<Position> | null>(null);

  const defaultColDef = useMemo<ColDef>(() => ({ sortable: true, resizable: true }), []);

  // A new datasource resets the grid's cache, so this reloads whenever filters change.
  useEffect(() => {
    if (!api) return;

    const datasource: IDatasource = {
      getRows: async (params) => {
        const sort = params.sortModel[0];
        try {
          const page = await fetchPositions(
            filters,
            params.startRow,
            params.endRow,
            sort?.colId,
            sort?.sort,
          );
          params.successCallback(page.rows, page.total);
        } catch (err) {
          console.error(err);
          params.failCallback();
        }
      },
    };

    api.setGridOption('datasource', datasource);
  }, [api, filters]);

  const saveColumnState = (e: { api: GridApi }) =>
    localStorage.setItem(COLUMN_STATE_KEY, JSON.stringify(e.api.getColumnState()));

  const onGridReady = (e: GridReadyEvent<Position>) => {
    try {
      const saved = localStorage.getItem(COLUMN_STATE_KEY);
      if (saved) e.api.applyColumnState({ state: JSON.parse(saved), applyOrder: true });
    } catch {
      localStorage.removeItem(COLUMN_STATE_KEY);
    }
    setApi(e.api);
  };

  return (
    <div className="grid-wrapper">
      <AgGridReact<Position>
        columnDefs={columns}
        defaultColDef={defaultColDef}
        rowModelType="infinite"
        cacheBlockSize={100}
        maxBlocksInCache={30}
        getRowId={(p) => p.data.tradeId}
        onGridReady={onGridReady}
        onRowClicked={(e) => e.data && onRowSelected(e.data)}
        onColumnMoved={saveColumnState}
        onColumnResized={(e) => e.finished && saveColumnState(e)}
        onSortChanged={saveColumnState}
      />
    </div>
  );
}
