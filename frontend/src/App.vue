<script setup>
import { useAuthStore } from './stores/auth';
import { useRouter } from 'vue-router';

const auth = useAuthStore();
const router = useRouter();

function handleLogout() {
  auth.logout();
  router.push({ name: 'login' });
}
</script>

<template>
  <div class="layout">
    <header class="topbar">
      <h1>🎵 Playlists</h1>
      <nav v-if="auth.isAuthenticated">
        <span class="user-email">{{ auth.user?.email }}</span>
        <button class="btn-link" @click="handleLogout">Cerrar sesión</button>
      </nav>
    </header>

    <main class="content">
      <router-view />
    </main>
  </div>
</template>

<style>
* { box-sizing: border-box; }
body { margin: 0; font-family: system-ui, sans-serif; background: #f5f5f7; }

.layout { min-height: 100vh; display: flex; flex-direction: column; }

.topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: #1f1147;
  color: white;
}

.topbar h1 { margin: 0; font-size: 1.2rem; }

.topbar nav { display: flex; align-items: center; gap: 1rem; }

.user-email { font-size: 0.9rem; opacity: 0.85; }

.btn-link {
  background: none;
  border: 1px solid rgba(255,255,255,0.4);
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  cursor: pointer;
}
.btn-link:hover { background: rgba(255,255,255,0.1); }

.content { flex: 1; padding: 2rem; max-width: 900px; margin: 0 auto; width: 100%; }
</style>
