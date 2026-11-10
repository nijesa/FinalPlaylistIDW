import axios from 'axios';

// La URL base viene de la variable de entorno VITE_API_URL (archivo .env).
// Para cambiar de backend (NestJS <-> ASP.NET) solo se edita el .env, el código no se toca.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

// Requisito 12: enviar el token en el header Authorization en cada request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Si el token expira o es inválido, cerramos sesión localmente
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
    }
    return Promise.reject(error);
  },
);

export default api;
