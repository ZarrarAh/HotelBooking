import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import type { Role } from '../types/api'
import { homeFor, useAuth } from './context'

/**
 * Sends anonymous visitors to the login page (remembering where they were going) and
 * users with the wrong role to their own home page. This is only for UX: the API enforces
 * the same rules on every request.
 */
export default function ProtectedRoute({ role, children }: { role: Role; children: ReactNode }) {
  const { user } = useAuth()
  const location = useLocation()

  if (!user) {
    const redirect = encodeURIComponent(location.pathname + location.search)
    return <Navigate to={`/login?redirect=${redirect}`} replace />
  }
  if (user.role !== role) {
    return <Navigate to={homeFor(user.role)} replace />
  }
  return children
}
