import { useEffect, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { roomsApi } from '../../api/endpoints'
import { useAuth } from '../../auth/context'
import ErrorMessage from '../../components/ErrorMessage'
import type { AvailableRoom } from '../../types/api'
import { addDays, formatDate, formatPrice, plural, todayIso } from '../../utils/format'

/**
 * The search criteria live in the URL (?checkIn=…&checkOut=…&guests=…), so results survive
 * a reload or the back button and can be shared as a link.
 */
export default function SearchPage() {
  const { user } = useAuth()
  const [params, setParams] = useSearchParams()
  const checkIn = params.get('checkIn')
  const checkOut = params.get('checkOut')
  const guests = Number(params.get('guests') ?? 1)

  // Form fields start from the URL, or from "tonight" if there's no search yet.
  const [form, setForm] = useState({
    checkIn: checkIn ?? todayIso(),
    checkOut: checkOut ?? addDays(todayIso(), 1),
    guests,
  })
  const [rooms, setRooms] = useState<AvailableRoom[] | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!checkIn || !checkOut) return
    // Ignore responses that arrive after the user has already started a newer search.
    let stale = false

    async function search(from: string, to: string) {
      setLoading(true)
      setError(null)
      try {
        const result = await roomsApi.available(from, to, guests)
        if (!stale) setRooms(result)
      } catch (err) {
        if (!stale) {
          setError(err)
          setRooms(null)
        }
      } finally {
        if (!stale) setLoading(false)
      }
    }

    search(checkIn, checkOut)
    return () => {
      stale = true
    }
  }, [checkIn, checkOut, guests])

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setParams({ checkIn: form.checkIn, checkOut: form.checkOut, guests: String(form.guests) })
  }

  function updateCheckIn(value: string) {
    // Keep check-out after check-in so the form can't describe an impossible stay.
    setForm((f) => ({ ...f, checkIn: value, checkOut: f.checkOut > value ? f.checkOut : addDays(value, 1) }))
  }

  const bookingQuery = checkIn && checkOut ? `?checkIn=${checkIn}&checkOut=${checkOut}&guests=${guests}` : ''

  return (
    <div className="stack">
      <div className="page-header">
        <h1>Find a room</h1>
        <p>Pick your dates and we'll show every room that's free for the whole stay.</p>
      </div>

      <form className="card inline" onSubmit={handleSubmit}>
        <label>
          Check-in
          <input
            type="date"
            value={form.checkIn}
            min={todayIso()}
            onChange={(e) => updateCheckIn(e.target.value)}
            required
          />
        </label>
        <label>
          Check-out
          <input
            type="date"
            value={form.checkOut}
            min={addDays(form.checkIn, 1)}
            onChange={(e) => setForm({ ...form, checkOut: e.target.value })}
            required
          />
        </label>
        <label>
          Guests
          <select value={form.guests} onChange={(e) => setForm({ ...form, guests: Number(e.target.value) })}>
            {[1, 2, 3, 4].map((n) => <option key={n} value={n}>{n}</option>)}
          </select>
        </label>
        <button type="submit" disabled={loading}>{loading ? 'Searching…' : 'Search'}</button>
      </form>

      <ErrorMessage error={error} />

      {rooms && checkIn && checkOut && (
        <>
          <p className="muted">
            {plural(rooms.length, 'room')} available from {formatDate(checkIn)} to {formatDate(checkOut)}
            {' '}for {plural(guests, 'guest')}.
          </p>
          {rooms.length === 0 ? (
            <div className="card empty">No rooms are free for those dates. Try different dates or fewer guests.</div>
          ) : (
            <div className="grid">
              {rooms.map((room) => (
                <article key={room.id} className="card stack">
                  <div className="row spread">
                    <h3>Room {room.number}</h3>
                    <span className="badge">{room.type}</span>
                  </div>
                  <p className="muted small">{room.description}</p>
                  <p className="small">Sleeps {room.capacity} · {formatPrice(room.pricePerNight)} / night</p>
                  <div className="row spread">
                    <div>
                      <div className="price">{formatPrice(room.totalPrice)}</div>
                      <div className="muted small">{plural(room.nights, 'night')}</div>
                    </div>
                    {user?.role !== 'Staff' && (
                      <Link className="button" to={`/book/${room.id}${bookingQuery}`}>Book</Link>
                    )}
                  </div>
                </article>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  )
}
