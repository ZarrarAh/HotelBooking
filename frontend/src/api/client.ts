import type { AuthResponse, ProblemDetails } from '../types/api'

const STORAGE_KEY = 'hotelbooking.auth'

/** An HTTP error from the API, with the most useful human-readable message pulled out. */
export class ApiError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(status: number, problem?: ProblemDetails) {
    const validation = problem?.errors && Object.values(problem.errors).flat()[0]
    super(problem?.detail ?? validation ?? problem?.title ?? `Request failed (${status})`)
    this.status = status
    this.problem = problem
  }
}

// --- Session storage (the JWT and the user it belongs to) ---

export function loadSession(): AuthResponse | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const session = JSON.parse(raw) as AuthResponse
    return new Date(session.expiresAtUtc) > new Date() ? session : null
  } catch {
    return null
  }
}

export function saveSession(session: AuthResponse | null) {
  try {
    if (session) localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Storage unavailable (e.g. private mode): the session just won't survive a reload.
  }
}

let onUnauthorized: () => void = () => {}

/** Lets the auth context sign the user out when the API rejects their token. */
export function setUnauthorizedHandler(handler: () => void) {
  onUnauthorized = handler
}

// --- Fetch wrapper ---

export async function api<T>(path: string, options: { method?: string; body?: unknown } = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  const token = loadSession()?.token
  if (token) headers.Authorization = `Bearer ${token}`
  if (options.body !== undefined) headers['Content-Type'] = 'application/json'

  const response = await fetch(`/api${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (!response.ok) {
    if (response.status === 401 && token) onUnauthorized()
    const problem = await response.json().catch(() => undefined)
    throw new ApiError(response.status, problem)
  }

  return response.status === 204 ? (undefined as T) : ((await response.json()) as T)
}
