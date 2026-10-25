import { createContext, useContext } from 'react'
import type { Role, User } from '../types/api'

export interface AuthContextValue {
  user: User | null
  login: (email: string, password: string) => Promise<User>
  register: (email: string, password: string, fullName: string) => Promise<User>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside <AuthProvider>')
  return value
}

/** Where each role lands after logging in. */
export function homeFor(role: Role): string {
  return role === 'Staff' ? '/staff' : '/'
}
