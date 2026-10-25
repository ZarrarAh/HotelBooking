import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { reservationsApi } from '../../api/endpoints'
import ErrorMessage from '../../components/ErrorMessage'
import StatusBadge from '../../components/StatusBadge'
import type { Reservation } from '../../types/api'
import { formatDate, formatPrice, plural, todayIso } from '../../utils/format'

export default function MyBookingsPage() {
  const [reservations, setReservations] = useState<Reservation[] | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [cancellingId, setCancellingId] = useState<number | null>(null)

  const load = useCallback(() => {
    reservationsApi.mine().then(setReservations).catch(setError)
  }, [])

  useEffect(load, [load])

  async function cancel(reservation: Reservation) {
    if (!confirm(`Cancel your stay in room ${reservation.roomNumber} on ${formatDate(reservation.checkIn)}?`)) return
    setCancellingId(reservation.id)
    setError(null)
    try {
      const updated = await reservationsApi.cancel(reservation.id)
      setReservations((list) => list?.map((r) => (r.id === updated.id ? updated : r)) ?? null)
    } catch (err) {
      setError(err)
    } finally {
      setCancellingId(null)
    }
  }

  if (!reservations) {
    return error ? <ErrorMessage error={error} /> : <p className="muted">Loading…</p>
  }

  // "Upcoming" = still ahead or in progress; everything else is history.
  const today = todayIso()
  const isActive = (r: Reservation) =>
    r.status === 'CheckedIn' || (r.status === 'Confirmed' && r.checkOut > today)
  const upcoming = reservations.filter(isActive).sort((a, b) => a.checkIn.localeCompare(b.checkIn))
  const past = reservations.filter((r) => !isActive(r))

  return (
    <div className="stack">
      <div className="page-header">
        <h1>My bookings</h1>
      </div>
      <ErrorMessage error={error} />

      <h2>Upcoming</h2>
      {upcoming.length === 0 ? (
        <div className="card empty">
          No upcoming stays. <Link to="/">Find a room</Link>
        </div>
      ) : (
        upcoming.map((r) => (
          <BookingCard key={r.id} reservation={r}>
            {r.status === 'Confirmed' && (
              <button className="danger" onClick={() => cancel(r)} disabled={cancellingId === r.id}>
                {cancellingId === r.id ? 'Cancelling…' : 'Cancel'}
              </button>
            )}
          </BookingCard>
        ))
      )}

      {past.length > 0 && (
        <>
          <h2>Past and cancelled</h2>
          {past.map((r) => <BookingCard key={r.id} reservation={r} />)}
        </>
      )}
    </div>
  )
}

function BookingCard({ reservation: r, children }: { reservation: Reservation; children?: ReactNode }) {
  return (
    <article className="card row spread">
      <div>
        <div className="row">
          <h3>Room {r.roomNumber}</h3>
          <StatusBadge status={r.status} />
        </div>
        <div>{formatDate(r.checkIn)} → {formatDate(r.checkOut)}</div>
        <div className="muted small">
          {plural(r.nights, 'night')} · {plural(r.numberOfGuests, 'guest')} · {r.roomType} · #{r.id}
        </div>
      </div>
      <div className="row">
        <span className="price">{formatPrice(r.totalPrice)}</span>
        {children}
      </div>
    </article>
  )
}
