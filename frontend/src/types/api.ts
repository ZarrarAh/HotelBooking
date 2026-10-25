// Mirrors the API's request/response contracts (backend/src/HotelBooking.Api/Contracts).
// Dates are ISO strings: "2026-12-01" for DateOnly, full ISO timestamps for DateTime.

export type Role = 'Guest' | 'Staff'
export type RoomType = 'Single' | 'Double' | 'Suite'
export type ReservationStatus = 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled'

export interface User {
  id: number
  email: string
  fullName: string
  role: Role
}

export interface AuthResponse {
  token: string
  expiresAtUtc: string
  user: User
}

export interface Room {
  id: number
  number: string
  type: RoomType
  capacity: number
  pricePerNight: number
  description: string | null
}

export interface AvailableRoom extends Room {
  nights: number
  totalPrice: number
}

export interface Reservation {
  id: number
  roomId: number
  roomNumber: string
  roomType: RoomType
  guestId: number
  guestName: string
  guestEmail: string
  checkIn: string
  checkOut: string
  nights: number
  numberOfGuests: number
  totalPrice: number
  status: ReservationStatus
  createdAtUtc: string
  checkedInAtUtc: string | null
  checkedOutAtUtc: string | null
  cancelledAtUtc: string | null
}

/** RFC 7807 error body returned by the API. */
export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}
