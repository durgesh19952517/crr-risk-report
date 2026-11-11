import { useEffect, useState } from 'react';
import type { FilterOptions, Filters } from '../types';

interface Props {
  filters: Filters;
  options: FilterOptions | null;
  onChange: (changes: Partial<Filters>) => void;
  onClear: () => void;
}

export function FilterBar({ filters, options, onChange, onClear }: Props) {
  const [text, setText] = useState(filters.search);

  // wait for the user to stop typing before hitting the API
  useEffect(() => {
    if (text === filters.search) return;
    const timer = setTimeout(() => onChange({ search: text }), 300);
    return () => clearTimeout(timer);
  }, [text]);

  // keep the box in sync when filters are cleared from outside
  useEffect(() => setText(filters.search), [filters.search]);

  const select = (field: keyof Filters, label: string, values: string[] = []) => (
    <select value={filters[field]} onChange={(e) => onChange({ [field]: e.target.value })}>
      <option value="">{label}</option>
      {values.map((v) => (
        <option key={v} value={v}>
          {v}
        </option>
      ))}
    </select>
  );

  return (
    <div className="filter-bar">
      <input
        type="search"
        placeholder="Search counterparty, commodity or trade id"
        value={text}
        onChange={(e) => setText(e.target.value)}
      />
      {select('book', 'All books', options?.books)}
      {select('commodity', 'All commodities', options?.commodities)}
      {select('region', 'All regions', options?.regions)}
      {select('instrumentType', 'All instruments', options?.instrumentTypes)}
      {select('currency', 'All currencies', options?.currencies)}
      <button onClick={onClear}>Clear</button>
    </div>
  );
}
