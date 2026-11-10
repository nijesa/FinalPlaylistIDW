import { defineStore } from 'pinia';
import { Preferences } from '@capacitor/preferences';
import api from '../services/api';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null,
    user: null,
    initialized: false, // evita que el router decida antes de leer Preferences
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
  },

  actions: {
    // Se llama una sola vez al arrancar la app (desde main.js),
    // para cargar la sesión guardada antes de mostrar cualquier pantalla.
    async init() {
      const { value: token } = await Preferences.get({ key: 'accessToken' });
      const { value: userRaw } = await Preferences.get({ key: 'user' });
      this.token = token || null;
      this.user = userRaw ? JSON.parse(userRaw) : null;
      this.initialized = true;
    },

    async register(email, password) {
      await api.post('/auth/register', { email, password });
    },

    async login(email, password) {
      const { data } = await api.post('/auth/login', { email, password });
      this.token = data.accessToken;
      this.user = data.user;
      // Requisito 7: persistir el token con Capacitor Preferences
      await Preferences.set({ key: 'accessToken', value: data.accessToken });
      await Preferences.set({ key: 'user', value: JSON.stringify(data.user) });
    },

    async logout() {
      this.token = null;
      this.user = null;
      await Preferences.remove({ key: 'accessToken' });
      await Preferences.remove({ key: 'user' });
    },
  },
});
