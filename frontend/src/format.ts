const whole = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 });
const twoDp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function formatWhole(value: number | null | undefined): string {
  return value == null ? '' : whole.format(value);
}

export function formatDecimal(value: number | null | undefined): string {
  return value == null ? '' : twoDp.format(value);
}
