import { Link, NavLink, Outlet } from 'react-router-dom'

export default function Layout() {
  return (
    <>
      <header className="navbar">
        <div className="navbar-inner">
          <Link to="/" className="brand">Harbor Hotel</Link>
          <nav className="nav-links">
            <NavLink to="/" end>Search</NavLink>
          </nav>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
    </>
  )
}
