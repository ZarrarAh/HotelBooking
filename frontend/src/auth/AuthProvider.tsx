import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { loadSession, saveSession, setUnauthorizedHandler } from '../api/client'
import { authApi } from '../api/endpoints'
import type { AuthResponse, User } from '../types/api'
import { AuthContext } from './context'

/** Holds the signed-in user and keeps the JWT in localStorage so it survives reloads. */
export default function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => loadSession()?.user ?? null)

  const startSession = useCallback((session: AuthResponse) => {
    saveSession(session)
    setUser(session.user)
    return session.user
  }, [])

  const logout = useCallback(() => {
    saveSession(null)
    setUser(null)
  }, [])

  // An expired or revoked token makes the API answer 401; treat that as being signed out.
  useEffect(() => setUnauthorizedHandler(logout), [logout])

  const value = useMemo(
    () => ({
      user,
      login: async (email: string, password: string) => startSession(await authApi.login(email, password)),
      register: async (email: string, password: string, fullName: string) =>
        startSession(await authApi.register(email, password, fullName)),
      logout,
    }),
    [user, startSession, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
