import { describe, expect, it } from 'vitest';
import { buildQuery } from './api';
import { formatWhole } from './format';

describe('buildQuery', () => {
  it('skips empty and undefined values', () => {
    expect(buildQuery({ book: 'Ags', region: '', sortBy: undefined, startRow: 0 })).toBe(
      'book=Ags&startRow=0',
    );
  });

  it('encodes special characters', () => {
    expect(buildQuery({ search: 'Pacific Bulk & Co' })).toBe('search=Pacific+Bulk+%26+Co');
  });
});

describe('formatWhole', () => {
  it('adds thousands separators', () => {
    expect(formatWhole(1234567.8)).toBe('1,234,568');
  });

  it('returns an empty string for missing values', () => {
    expect(formatWhole(undefined)).toBe('');
    expect(formatWhole(null)).toBe('');
  });
});
