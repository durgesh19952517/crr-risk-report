import { useEffect, useState } from 'react';
import { fetchFilterOptions } from './api';
import { FilterBar } from './components/FilterBar';
import { PositionGrid } from './components/PositionGrid';
import { SummaryPanel } from './components/SummaryPanel';
import { DetailPanel } from './components/DetailPanel';
import { emptyFilters, type FilterOptions, type Filters, type Position } from './types';

export default function App() {
  const [filters, setFilters] = useState<Filters>(emptyFilters);
  const [options, setOptions] = useState<FilterOptions | null>(null);
  const [selected, setSelected] = useState<Position | null>(null);

  useEffect(() => {
    fetchFilterOptions().then(setOptions).catch(console.error);
  }, []);

  const updateFilters = (changes: Partial<Filters>) =>
    setFilters((current) => ({ ...current, ...changes }));

  return (
    <div className="app">
      <header>
        <h1>Risk Positions</h1>
      </header>

      <FilterBar
        filters={filters}
        options={options}
        onChange={updateFilters}
        onClear={() => setFilters(emptyFilters)}
      />

      <main>
        <section className="grid-area">
          <PositionGrid filters={filters} onRowSelected={setSelected} />
        </section>

        <aside>
          <SummaryPanel filters={filters} onDrill={updateFilters} />
          <DetailPanel position={selected} onClose={() => setSelected(null)} />
        </aside>
      </main>
    </div>
  );
}
