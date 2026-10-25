import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<h1>Find a room</h1>} />
          <Route path="*" element={<p className="empty">Page not found.</p>} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
