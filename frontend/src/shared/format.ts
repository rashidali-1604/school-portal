const currency = new Intl.NumberFormat('en-AU', { style: 'currency', currency: 'AUD' });
const date = new Intl.DateTimeFormat('en-AU', {
  dateStyle: 'medium',
  timeStyle: 'short'
});

export function formatMoney(value: number): string {
  return currency.format(value);
}

export function formatDateTime(value: string | Date): string {
  return date.format(typeof value === 'string' ? new Date(value) : value);
}
