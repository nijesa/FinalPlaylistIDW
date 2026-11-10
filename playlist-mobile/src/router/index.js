import { createRouter, createWebHistory } from '@ionic/vue-router';
import { useAuthStore } from '../stores/auth';

const routes = [
  { path: '/', redirect: '/songs' },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginPage.vue'),
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/RegisterPage.vue'),
  },
  {
    path: '/songs',
    name: 'songs',
    component: () => import('../views/SongsPage.vue'),
    meta: { requiresAuth: true },
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

// Nota: para cuando este guard corre, authStore.init() ya se ejecutó en main.js,
// así que el token (leído de Capacitor Preferences) ya está disponible en memoria.
router.beforeEach((to) => {
  const authStore = useAuthStore();

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login' };
  }

  if ((to.name === 'login' || to.name === 'register') && authStore.isAuthenticated) {
    return { name: 'songs' };
  }

  return true;
});

export default router;
