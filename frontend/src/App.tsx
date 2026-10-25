import { BrowserRouter, Route, Routes } from 'react-router-dom'
import AuthProvider from './auth/AuthProvider'
import ProtectedRoute from './auth/ProtectedRoute'
import Layout from './components/Layout'
import BookPage from './pages/guest/BookPage'
import MyBookingsPage from './pages/guest/MyBookingsPage'
import SearchPage from './pages/guest/SearchPage'
import FrontDeskPage from './pages/staff/FrontDeskPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route index element={<SearchPage />} />
            <Route path="login" element={<LoginPage />} />
            <Route path="register" element={<RegisterPage />} />
            <Route
              path="book/:roomId"
              element={<ProtectedRoute role="Guest"><BookPage /></ProtectedRoute>}
            />
            <Route
              path="my-bookings"
              element={<ProtectedRoute role="Guest"><MyBookingsPage /></ProtectedRoute>}
            />
            <Route
              path="staff"
              element={<ProtectedRoute role="Staff"><FrontDeskPage /></ProtectedRoute>}
            />
            <Route path="*" element={<p className="empty">Page not found.</p>} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
