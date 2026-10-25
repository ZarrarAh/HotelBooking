import type { ReservationStatus } from '../types/api'

const labels: Record<ReservationStatus, string> = {
  Confirmed: 'Confirmed',
  CheckedIn: 'Checked in',
  CheckedOut: 'Checked out',
  Cancelled: 'Cancelled',
}

export default function StatusBadge({ status }: { status: ReservationStatus }) {
  return <span className={`badge ${status}`}>{labels[status]}</span>
}
