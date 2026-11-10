<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
  IonItem, IonLabel, IonInput, IonButton, IonText,
} from '@ionic/vue';
import { useAuthStore } from '../stores/auth';

const email = ref('');
const password = ref('');
const error = ref('');
const loading = ref(false);

const auth = useAuthStore();
const router = useRouter();

async function handleLogin() {
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
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-title>Iniciar sesión</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <ion-item>
        <ion-label position="stacked">Correo</ion-label>
        <ion-input v-model="email" type="email" placeholder="tu@correo.com" required />
      </ion-item>

      <ion-item>
        <ion-label position="stacked">Contraseña</ion-label>
        <ion-input v-model="password" type="password" placeholder="••••••••" required />
      </ion-item>

      <ion-text color="danger" v-if="error">
        <p class="ion-padding-start">{{ error }}</p>
      </ion-text>

      <ion-button expand="block" class="ion-margin-top" :disabled="loading" @click="handleLogin">
        {{ loading ? 'Ingresando...' : 'Ingresar' }}
      </ion-button>

      <ion-button expand="block" fill="clear" router-link="/register">
        ¿No tienes cuenta? Regístrate
      </ion-button>
    </ion-content>
  </ion-page>
</template>
