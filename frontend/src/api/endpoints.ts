import type { AuthResponse, AvailableRoom, Reservation, Room, User } from '../types/api'
import { api } from './client'

export const authApi = {
  login: (email: string, password: string) =>
    api<AuthResponse>('/auth/login', { method: 'POST', body: { email, password } }),
  register: (email: string, password: string, fullName: string) =>
    api<AuthResponse>('/auth/register', { method: 'POST', body: { email, password, fullName } }),
  me: () => api<User>('/auth/me'),
}

export const roomsApi = {
  get: (id: number) => api<Room>(`/rooms/${id}`),
  available: (checkIn: string, checkOut: string, guests: number) =>
    api<AvailableRoom[]>(
      `/rooms/available?${new URLSearchParams({ checkIn, checkOut, guests: String(guests) })}`,
    ),
}

export const reservationsApi = {
  create: (roomId: number, checkIn: string, checkOut: string, numberOfGuests: number) =>
    api<Reservation>('/reservations', { method: 'POST', body: { roomId, checkIn, checkOut, numberOfGuests } }),
  mine: () => api<Reservation[]>('/reservations/mine'),
  cancel: (id: number) => api<Reservation>(`/reservations/${id}/cancel`, { method: 'POST' }),
}

export const staffApi = {
  arrivals: (date: string) => api<Reservation[]>(`/staff/arrivals?date=${date}`),
  departures: (date: string) => api<Reservation[]>(`/staff/departures?date=${date}`),
  checkIn: (id: number) => api<Reservation>(`/staff/reservations/${id}/check-in`, { method: 'POST' }),
  checkOut: (id: number) => api<Reservation>(`/staff/reservations/${id}/check-out`, { method: 'POST' }),
}
