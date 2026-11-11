import { formatDecimal } from '../format';
import type { Position } from '../types';

interface Props {
  position: Position | null;
  onClose: () => void;
}

export function DetailPanel({ position, onClose }: Props) {
  if (!position) {
    return (
      <div className="panel">
        <h2>Trade detail</h2>
        <p className="hint">Click a row in the grid to see its details.</p>
      </div>
    );
  }

  const rows: [string, string][] = [
    ['Trade ID', position.tradeId],
    ['Book', position.book],
    ['Commodity', position.commodity],
    ['Counterparty', position.counterparty],
    ['Region', position.region],
    ['Instrument', position.instrumentType],
    ['Position', formatDecimal(position.position)],
    ['Currency', position.currency],
    ['Market price', formatDecimal(position.marketPrice)],
    ['P&L (USD)', formatDecimal(position.pnl)],
    ['FX exposure (USD)', formatDecimal(position.fxExposure)],
    ['As of', position.asOfDate],
  ];

  return (
    <div className="panel">
      <div className="panel-header">
        <h2>Trade detail</h2>
        <button onClick={onClose}>Close</button>
      </div>
      <dl>
        {rows.map(([label, value]) => (
          <div key={label}>
            <dt>{label}</dt>
            <dd>{value}</dd>
          </div>
        ))}
      </dl>
    </div>
  );
}
