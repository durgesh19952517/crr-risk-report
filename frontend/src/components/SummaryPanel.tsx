import { useEffect, useState } from 'react';
import { fetchSummary } from '../api';
import { formatWhole } from '../format';
import type { Filters, GroupField, SummaryResult } from '../types';

interface Props {
  filters: Filters;
  onDrill: (changes: Partial<Filters>) => void;
}

const groupOptions: { value: GroupField; label: string }[] = [
  { value: 'book', label: 'Book' },
  { value: 'commodity', label: 'Commodity' },
  { value: 'region', label: 'Region' },
  { value: 'instrumentType', label: 'Instrument' },
];

export function SummaryPanel({ filters, onDrill }: Props) {
  const [groupBy, setGroupBy] = useState<GroupField>('book');
  const [data, setData] = useState<SummaryResult | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    const controller = new AbortController();
    setError('');

    fetchSummary(filters, groupBy, controller.signal)
      .then(setData)
      .catch((err) => {
        if (err.name !== 'AbortError') setError('Could not load summary');
      });

    return () => controller.abort();
  }, [filters, groupBy]);

  const showPosition = groupBy === 'commodity';
  const sign = (n: number) => (n < 0 ? 'negative' : 'positive');

  return (
    <div className="panel">
      <div className="panel-header">
        <h2>Summary</h2>
        <select value={groupBy} onChange={(e) => setGroupBy(e.target.value as GroupField)}>
          {groupOptions.map((o) => (
            <option key={o.value} value={o.value}>
              By {o.label.toLowerCase()}
            </option>
          ))}
        </select>
      </div>

      {error && <p className="error">{error}</p>}

      {data && (
        <table>
          <thead>
            <tr>
              <th>{groupOptions.find((o) => o.value === groupBy)?.label}</th>
              <th>Trades</th>
              <th>P&amp;L</th>
              <th>FX exp.</th>
              {showPosition && <th>Position</th>}
            </tr>
          </thead>
          <tbody>
            {data.groups.map((g) => (
              <tr
                key={g.key}
                className={filters[groupBy] === g.key ? 'active clickable' : 'clickable'}
                onClick={() => onDrill({ [groupBy]: filters[groupBy] === g.key ? '' : g.key })}
              >
                <td>{g.key}</td>
                <td>{formatWhole(g.count)}</td>
                <td className={sign(g.pnl)}>{formatWhole(g.pnl)}</td>
                <td className={sign(g.fxExposure)}>{formatWhole(g.fxExposure)}</td>
                {showPosition && <td>{formatWhole(g.position)}</td>}
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr>
              <td>Total</td>
              <td>{formatWhole(data.totals.count)}</td>
              <td className={sign(data.totals.pnl)}>{formatWhole(data.totals.pnl)}</td>
              <td className={sign(data.totals.fxExposure)}>{formatWhole(data.totals.fxExposure)}</td>
              {showPosition && <td></td>}
            </tr>
          </tfoot>
        </table>
      )}

      <p className="hint">Click a row to filter the grid to that group.</p>
      {showPosition && (
        <p className="hint">Position is in each commodity's own units, so there is no overall total.</p>
      )}
    </div>
  );
}
