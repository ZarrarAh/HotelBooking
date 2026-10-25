import { useEffect, useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { ApiError } from '../../api/client'
import { reservationsApi, roomsApi } from '../../api/endpoints'
import ErrorMessage from '../../components/ErrorMessage'
import type { Reservation, Room } from '../../types/api'
import { formatDate, formatPrice, plural } from '../../utils/format'

export default function BookPage() {
  const roomId = Number(useParams().roomId)
  const [params] = useSearchParams()
  const checkIn = params.get('checkIn') ?? ''
  const checkOut = params.get('checkOut') ?? ''
  const guests = Number(params.get('guests') ?? 1)

  const [room, setRoom] = useState<Room | null>(null)
  const [reservation, setReservation] = useState<Reservation | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    roomsApi.get(roomId).then(setRoom).catch(setError)
  }, [roomId])

  async function confirm() {
    setBusy(true)
    setError(null)
    try {
      setReservation(await reservationsApi.create(roomId, checkIn, checkOut, guests))
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  const searchLink = `/?checkIn=${checkIn}&checkOut=${checkOut}&guests=${guests}`

  if (!checkIn || !checkOut) {
    return <div className="card empty">Choose your dates first. <Link to="/">Search rooms</Link></div>
  }

  if (reservation) {
    return (
      <div className="card stack auth-card">
        <div className="alert success">Booking confirmed!</div>
        <h1>Room {reservation.roomNumber}</h1>
        <Summary checkIn={reservation.checkIn} checkOut={reservation.checkOut} nights={reservation.nights}
          guests={reservation.numberOfGuests} total={reservation.totalPrice} />
        <p className="muted small">Booking reference #{reservation.id}</p>
        <Link to="/my-bookings" className="button">View my bookings</Link>
      </div>
    )
  }

  // 409 means someone else got the room first (possibly seconds ago), so steer back to search.
  const conflict = error instanceof ApiError && error.status === 409

  if (!room) {
    return error ? <ErrorMessage error={error} /> : <p className="muted">Loading…</p>
  }

  const nights = Math.round((Date.parse(checkOut) - Date.parse(checkIn)) / 86_400_000)

  return (
    <div className="card stack auth-card">
      <h1>Confirm your booking</h1>
      <div>
        <div className="row spread">
          <h3>Room {room.number}</h3>
          <span className="badge">{room.type}</span>
        </div>
        <p className="muted small">{room.description}</p>
      </div>
      <Summary checkIn={checkIn} checkOut={checkOut} nights={nights} guests={guests}
        total={room.pricePerNight * nights} />

      {conflict ? (
        <div className="alert error">
          Sorry, this room was just booked by someone else for those dates.{' '}
          <Link to={searchLink}>See other rooms</Link>
        </div>
      ) : (
        <ErrorMessage error={error} />
      )}

      <div className="row">
        <button onClick={confirm} disabled={busy || conflict}>{busy ? 'Booking…' : 'Confirm booking'}</button>
        <Link to={searchLink}>Back to results</Link>
      </div>
    </div>
  )
}

function Summary(props: { checkIn: string; checkOut: string; nights: number; guests: number; total: number }) {
  return (
    <table>
      <tbody>
        <tr><th>Check-in</th><td>{formatDate(props.checkIn)}</td></tr>
        <tr><th>Check-out</th><td>{formatDate(props.checkOut)}</td></tr>
        <tr><th>Stay</th><td>{plural(props.nights, 'night')}, {plural(props.guests, 'guest')}</td></tr>
        <tr><th>Total</th><td className="price">{formatPrice(props.total)}</td></tr>
      </tbody>
    </table>
  )
}
