import { useCallback, useEffect, useState } from 'react'
import { staffApi } from '../../api/endpoints'
import ErrorMessage from '../../components/ErrorMessage'
import StatusBadge from '../../components/StatusBadge'
import type { Reservation } from '../../types/api'
import { addDays, formatDate, todayIso } from '../../utils/format'

type Tab = 'arrivals' | 'departures'

export default function FrontDeskPage() {
  const [date, setDate] = useState(todayIso())
  const [tab, setTab] = useState<Tab>('arrivals')
  const [arrivals, setArrivals] = useState<Reservation[] | null>(null)
  const [departures, setDepartures] = useState<Reservation[] | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = useCallback(async () => {
    setError(null)
    try {
      const [a, d] = await Promise.all([staffApi.arrivals(date), staffApi.departures(date)])
      setArrivals(a)
      setDepartures(d)
    } catch (err) {
      setError(err)
    }
  }, [date])

  useEffect(() => {
    load()
  }, [load])

  async function run(action: (id: number) => Promise<Reservation>, reservation: Reservation) {
    setBusyId(reservation.id)
    setError(null)
    try {
      await action(reservation.id)
      // A check-in can also change the departures list (and vice versa), so refresh both.
      await load()
    } catch (err) {
      setError(err)
    } finally {
      setBusyId(null)
    }
  }

  const list = tab === 'arrivals' ? arrivals : departures
  const pending = (items: Reservation[] | null, status: Reservation['status']) =>
    items?.filter((r) => r.status === status).length ?? 0

  return (
    <div className="stack">
      <div className="page-header row spread">
        <div>
          <h1>Front desk</h1>
          <p>{formatDate(date)}{date === todayIso() && ' (today)'}</p>
        </div>
        <div className="row">
          <button className="secondary" onClick={() => setDate(addDays(date, -1))} aria-label="Previous day">←</button>
          <input type="date" value={date} onChange={(e) => e.target.value && setDate(e.target.value)} />
          <button className="secondary" onClick={() => setDate(addDays(date, 1))} aria-label="Next day">→</button>
          {date !== todayIso() && <button className="link" onClick={() => setDate(todayIso())}>Today</button>}
        </div>
      </div>

      <ErrorMessage error={error} />

      <div className="card">
        <div className="tabs" role="tablist">
          <button role="tab" aria-selected={tab === 'arrivals'} className={tab === 'arrivals' ? 'active' : ''}
            onClick={() => setTab('arrivals')}>
            Arrivals ({pending(arrivals, 'Confirmed')} to check in)
          </button>
          <button role="tab" aria-selected={tab === 'departures'} className={tab === 'departures' ? 'active' : ''}
            onClick={() => setTab('departures')}>
            Departures ({pending(departures, 'CheckedIn')} to check out)
          </button>
        </div>

        {!list ? (
          <p className="muted">Loading…</p>
        ) : list.length === 0 ? (
          <p className="empty">No {tab} on this day.</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Room</th>
                  <th>Guest</th>
                  <th>Stay</th>
                  <th>Guests</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {list.map((r) => (
                  <tr key={r.id}>
                    <td><strong>{r.roomNumber}</strong><div className="muted small">{r.roomType}</div></td>
                    <td>{r.guestName}<div className="muted small">{r.guestEmail}</div></td>
                    <td className="small">{formatDate(r.checkIn)} → {formatDate(r.checkOut)}</td>
                    <td>{r.numberOfGuests}</td>
                    <td><StatusBadge status={r.status} /></td>
                    <td>
                      {tab === 'arrivals' && r.status === 'Confirmed' && (
                        <button onClick={() => run(staffApi.checkIn, r)} disabled={busyId === r.id}>
                          {busyId === r.id ? 'Checking in…' : 'Check in'}
                        </button>
                      )}
                      {tab === 'departures' && r.status === 'CheckedIn' && (
                        <button onClick={() => run(staffApi.checkOut, r)} disabled={busyId === r.id}>
                          {busyId === r.id ? 'Checking out…' : 'Check out'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
