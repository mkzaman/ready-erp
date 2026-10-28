export function formatNumber(value: number | null | undefined): string {
  if (value == null) return ''
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return ''
  const d = new Date(value)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${pad(d.getDate())}.${pad(d.getMonth() + 1)}.${d.getFullYear()}`
}
