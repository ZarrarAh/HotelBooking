# Hotel Booking – Web

React + TypeScript + Vite frontend for the Hotel Booking API.

```bash
npm install
npm run dev      # http://localhost:5173, proxies /api to the API on http://localhost:5284
npm run build    # type-checks and builds to dist/
```

Set `API_PROXY_TARGET` to point the dev proxy at a different API, for example the Docker one:
`API_PROXY_TARGET=http://localhost:8080 npm run dev`.
