/** Today's date as "YYYY-MM-DD" in the browser's local time zone. */
export function todayIso(): string {
  return toIso(new Date())
}

export function addDays(iso: string, days: number): string {
  const date = parseIso(iso)
  date.setDate(date.getDate() + days)
  return toIso(date)
}

/** "2026-12-01" → "Tue, Dec 1, 2026" */
export function formatDate(iso: string): string {
  return parseIso(iso).toLocaleDateString(undefined, {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

export function formatPrice(amount: number): string {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EUR' }).format(amount)
}

export function plural(count: number, word: string): string {
  return `${count} ${word}${count === 1 ? '' : 's'}`
}

// Parse as a local date; new Date("2026-12-01") would be UTC midnight and can shift a day.
function parseIso(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number)
  return new Date(y, m - 1, d)
}

function toIso(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}
