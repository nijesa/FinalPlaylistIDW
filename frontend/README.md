# Frontend – Playlists (Vue 3 + Vite + Pinia)

## Requisitos
- Node.js 18+
- El backend corriendo en `http://localhost:3000/api`

## Instalación

```bash
npm install
npm run dev
```

Se abre en `http://localhost:5173`.

## Qué incluye
- `SongsView.vue`: listado, búsqueda por nombre (con debounce), paginación y CRUD completo.
- `LoginView.vue` / `RegisterView.vue`: formularios de autenticación.
- `stores/auth.js`: guarda el token en `localStorage` y lo expone a toda la app.
- `services/api.js`: instancia de axios que agrega automáticamente `Authorization: Bearer <token>` a cada request.
- `router/index.js`: navigation guard que bloquea `/songs` sin sesión y redirige `/login` y `/register` si ya hay sesión.
