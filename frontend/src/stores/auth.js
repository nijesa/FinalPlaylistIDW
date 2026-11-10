import { defineStore } from 'pinia';
import api from '../services/api';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('accessToken') || null,
    user: JSON.parse(localStorage.getItem('user') || 'null'),
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
  },

  actions: {
    async register(email, password) {
      await api.post('/auth/register', { email, password });
    },

    async login(email, password) {
      const { data } = await api.post('/auth/login', { email, password });
      this.token = data.accessToken;
      this.user = data.user;
      // Requisito 12: persistir el token
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('user', JSON.stringify(data.user));
    },

    // Requisito 14: cierre de sesión
    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
    },
  },
});
