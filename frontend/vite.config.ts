import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // The browser only ever talks to the Vite server; /api is forwarded to ASP.NET.
    // Same-origin requests mean the API needs no CORS setup (nginx does the same in Docker).
    proxy: {
      '/api': process.env.API_PROXY_TARGET ?? 'http://localhost:5284',
    },
  },
})
