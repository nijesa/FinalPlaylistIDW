import axios from 'axios';
import { Capacitor } from '@capacitor/core';
import { Preferences } from '@capacitor/preferences';

// La URL base viene de la variable de entorno VITE_API_URL (archivo .env).
// "localhost" solo funciona en navegador. En emulador Android, "localhost" apunta al propio
// emulador, así que se reemplaza por 10.0.2.2 (la PC que lo hospeda). En un celular físico,
// pon en el .env la IP de tu PC en la red local, ej: http://192.168.1.50:5080/api
function resolveBaseUrl() {
  const url = import.meta.env.VITE_API_URL;

  if (Capacitor.isNativePlatform() && Capacitor.getPlatform() === 'android') {
    return url.replace('localhost', '10.0.2.2');
  }
  return url;
}

const api = axios.create({
  baseURL: resolveBaseUrl(),
});

// Requisito 8: header Authorization centralizado.
// Como Preferences es async, el interceptor también lo es (axios lo soporta).
api.interceptors.request.use(async (config) => {
  const { value: token } = await Preferences.get({ key: 'accessToken' });
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response && error.response.status === 401) {
      await Preferences.remove({ key: 'accessToken' });
      await Preferences.remove({ key: 'user' });
    }
    return Promise.reject(error);
  },
);

export default api;
