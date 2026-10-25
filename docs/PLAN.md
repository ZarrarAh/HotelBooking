# Hotel Booking System: Plan

## Context
This is a portfolio project for junior .NET developer roles. The repo is empty: there is a git repo on `master` with no commits. The goal is a full-stack hotel booking system: an ASP.NET Core (.NET 10) API with PostgreSQL, a React + TypeScript + Vite frontend, and Docker Compose to run it all. You want to understand the architecture, so we build it in small steps and the backend is finished first. After each step I'll give a short high-level explanation of what changed and why.

## Key design decisions (keeping it simple on purpose)
- **Three backend projects plus tests**, not full "Clean Architecture" with mediators. That's enough layering to show separation of concerns without too much ceremony:
  - `Domain`: entities, enums, business rules. No dependencies.
  - `Infrastructure`: EF Core `DbContext`, entity configurations, migrations.
  - `Api`: controllers, DTOs, services, auth, Swagger.
- **Dates are `DateOnly`** with a half-open range `[checkIn, checkOut)`. One guest's checkout day can be the next guest's check-in day.
- **Double-booking protection uses two layers:**
  1. Service-level overlap check, which gives a friendly `409 Conflict` message.
  2. A **PostgreSQL exclusion constraint** (`btree_gist`, `EXCLUDE USING gist (room_id WITH =, daterange(check_in, check_out) WITH &&) WHERE status <> 'Cancelled'`). The database itself refuses overlapping bookings, even when two requests arrive at the same moment. We catch that error and turn it into a 409 as well. This makes a good interview talking point.
- **Auth:** a simple custom `User` table (Email, PasswordHash, Role), hashed with ASP.NET's `PasswordHasher<T>`, and JWT bearer tokens. Roles are `Guest` and `Staff`. This is easier to explain than full ASP.NET Identity. Staff accounts are seeded; guests self-register.
- **Reservation lifecycle:** `Confirmed → CheckedIn → CheckedOut`, or `Confirmed → Cancelled`. The `Reservation` entity enforces these rules, so invalid transitions throw a domain exception and the API maps it to `400`/`409`.
- **Errors:** one global exception handler that returns `ProblemDetails`.
- **Tests:** xUnit. Unit tests cover the domain rules. Integration tests use `WebApplicationFactory` + **Testcontainers (real PostgreSQL)**, so the exclusion constraint is actually exercised.
- **Frontend:** React Router, a small `fetch` API client, a JWT kept in an auth context (localStorage), and role-based route guards. Plain CSS or a light library. No Redux.

## Folder structure
```
HotelBooking/
├─ docker-compose.yml
├─ .env.example
├─ README.md
├─ backend/
│  ├─ HotelBooking.sln
│  ├─ Dockerfile
│  ├─ src/
│  │  ├─ HotelBooking.Domain/          # Room, Reservation, User, enums, DomainException
│  │  ├─ HotelBooking.Infrastructure/  # AppDbContext, Configurations/, Migrations/, Seed
│  │  └─ HotelBooking.Api/
│  │     ├─ Controllers/               # Auth, Rooms, Reservations, Staff
│  │     ├─ Contracts/                 # request/response DTOs
│  │     ├─ Services/                  # AuthService, RoomService, ReservationService
│  │     ├─ Auth/                      # JWT options, token generator
│  │     ├─ Middleware/                # exception → ProblemDetails
│  │     └─ Program.cs
│  └─ tests/
│     ├─ HotelBooking.UnitTests/
│     └─ HotelBooking.IntegrationTests/
└─ frontend/
   ├─ Dockerfile                       # build with Node, serve with nginx
   ├─ nginx.conf                       # serves SPA, proxies /api → backend
   ├─ package.json, vite.config.ts
   └─ src/
      ├─ api/                          # client.ts, rooms.ts, reservations.ts, auth.ts
      ├─ auth/                         # AuthContext, ProtectedRoute
      ├─ pages/
      │  ├─ guest/                     # SearchPage, BookPage, MyBookingsPage
      │  ├─ staff/                     # TodayPage (arrivals/departures + actions)
      │  └─ LoginPage, RegisterPage
      ├─ components/
      └─ types/
```

## API surface (target)
| Method | Route | Role |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | anonymous |
| GET | `/api/rooms`, `/api/rooms/{id}` | anonymous |
| GET | `/api/rooms/available?checkIn&checkOut&guests` | anonymous |
| POST/PUT/DELETE | `/api/rooms` | Staff |
| POST | `/api/reservations` | Guest |
| GET | `/api/reservations/mine` | Guest |
| POST | `/api/reservations/{id}/cancel` | Guest (own) / Staff |
| GET | `/api/staff/arrivals?date`, `/api/staff/departures?date` | Staff |
| POST | `/api/reservations/{id}/check-in`, `/check-out` | Staff |

## Steps (one at a time)

### Backend
1. **Solution skeleton.** Create the `.sln`, the 3 src projects and 2 test projects, the project references, `.gitignore`, and an empty API that runs with Swagger.
2. **Docker Compose with PostgreSQL only.** Add the `db` service, a volume, and `.env.example`. Point the API's connection string at it.
3. **Domain model.** Add `Room`, `Reservation` (with status transition methods), `User`, the enums, and `DomainException`. Add unit tests for the transition rules.
4. **EF Core + first migration.** Add `AppDbContext`, entity configurations, `DateOnly` mapping, the initial migration, and seed data (rooms plus one staff user). Apply migrations on startup in Development.
5. **Rooms endpoints.** Add read endpoints (list/get) and DTOs. Staff CRUD comes after auth.
6. **Availability search.** Find rooms with enough capacity that have no non-cancelled reservation overlapping `[checkIn, checkOut)`. Add input validation.
7. **JWT authentication.** Add register/login, password hashing, the token generator, role claims, `[Authorize(Roles=...)]`, and the Swagger "Authorize" button. Lock down the staff room CRUD.
8. **Reservations.** Add create (overlap check + price calculation), my bookings, and cancel. Guests can only see and cancel their own bookings.
9. **Double-booking protection at the DB level.** Add a migration with `btree_gist` and the exclusion constraint. Map the Postgres error `23P01` to `409`.
10. **Staff operations.** Add today's arrivals/departures, check-in and check-out, using the domain rules from step 3.
11. **Error handling and polish.** Add the global `ProblemDetails` handler, consistent validation responses, CORS for the frontend dev server, and Swagger XML comments.
12. **Integration tests.** Use Testcontainers PostgreSQL. Cover auth, availability, booking, **concurrent double-booking attempts (one succeeds, one gets 409)**, and check-in/out flows.
13. **Dockerize the API.** Add a multi-stage `Dockerfile` and an `api` service in Compose that depends on a healthy `db`.

### Frontend
14. **Vite scaffold.** React + TS, React Router, folder layout, API client with base URL and auth header.
15. **Auth.** Login and register pages, `AuthContext`, role-aware navbar, `ProtectedRoute`.
16. **Guest: search.** Date and guest-count form, then a list of available rooms.
17. **Guest: book.** Booking confirmation page, handling the 409 "just got booked" case.
18. **Guest: my bookings.** List with status and a cancel button.
19. **Staff: today.** Arrivals and departures tabs with Check-in / Check-out buttons and a date picker.
20. **Dockerize the frontend.** Node build, then nginx serving the app with a `/api` reverse proxy. Add a `web` service in Compose.

### Wrap-up
21. **README.** Architecture diagram, how to run, seeded credentials, screenshots, and the "how double booking is prevented" explanation.
22. *(Optional)* A GitHub Actions CI workflow that runs `dotnet test` and `npm run build`.

## Verification
- After each backend step: `dotnet build` and `dotnet test` pass, and the new endpoints get a manual check in Swagger.
- Step 12: the integration test suite runs against a real Postgres container. The concurrency test proves the constraint works.
- Final: `docker compose up --build` from the repo root. Then open the frontend, register a guest, search, book, and view bookings. Log in as the seeded staff user, check the guest in, then check them out.
