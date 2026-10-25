import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/context'

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <>
      <header className="navbar">
        <div className="navbar-inner">
          <Link to={user?.role === 'Staff' ? '/staff' : '/'} className="brand">Harbor Hotel</Link>
          <nav className="nav-links">
            {user?.role === 'Staff' ? (
              <NavLink to="/staff">Front desk</NavLink>
            ) : (
              <>
                <NavLink to="/" end>Search</NavLink>
                {user && <NavLink to="/my-bookings">My bookings</NavLink>}
              </>
            )}
          </nav>
          <div className="nav-user">
            {user ? (
              <>
                <span>{user.fullName}{user.role === 'Staff' && ' · Staff'}</span>
                <button className="secondary" onClick={handleLogout}>Log out</button>
              </>
            ) : (
              <>
                <Link to="/login">Log in</Link>
                <Link to="/register" className="button">Sign up</Link>
              </>
            )}
          </div>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
    </>
  )
}
