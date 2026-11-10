<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const email = ref('');
const password = ref('');
const error = ref('');
const loading = ref(false);

const auth = useAuthStore();
const router = useRouter();

async function handleSubmit() {
  error.value = '';
  loading.value = true;
  try {
    await auth.login(email.value, password.value);
    router.push({ name: 'songs' });
  } catch (err) {
    error.value = err.response?.data?.message || 'No se pudo iniciar sesión';
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="auth-card">
    <h2>Iniciar sesión</h2>
    <form @submit.prevent="handleSubmit">
      <label>
        Correo
        <input v-model="email" type="email" required placeholder="tu@correo.com" />
      </label>
      <label>
        Contraseña
        <input v-model="password" type="password" required placeholder="••••••••" />
      </label>

      <p v-if="error" class="error">{{ error }}</p>

      <button type="submit" :disabled="loading">
        {{ loading ? 'Ingresando...' : 'Ingresar' }}
      </button>
    </form>

    <p class="switch">
      ¿No tienes cuenta?
      <router-link :to="{ name: 'register' }">Regístrate</router-link>
    </p>
  </div>
</template>

<style scoped>
.auth-card {
  max-width: 380px;
  margin: 3rem auto;
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
}
form { display: flex; flex-direction: column; gap: 1rem; }
label { display: flex; flex-direction: column; gap: 0.35rem; font-size: 0.9rem; }
input {
  padding: 0.6rem 0.7rem;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 1rem;
}
button {
  padding: 0.7rem;
  background: #1f1147;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
}
button:disabled { opacity: 0.6; cursor: not-allowed; }
.error { color: #c0392b; font-size: 0.9rem; margin: 0; }
.switch { text-align: center; margin-top: 1rem; font-size: 0.9rem; }
</style>
