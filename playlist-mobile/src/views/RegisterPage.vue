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
const success = ref(false);
const loading = ref(false);

const auth = useAuthStore();
const router = useRouter();

async function handleRegister() {
  error.value = '';
  loading.value = true;
  try {
    await auth.register(email.value, password.value);
    success.value = true;
    setTimeout(() => router.push({ name: 'login' }), 1200);
  } catch (err) {
    error.value = err.response?.data?.message || 'No se pudo registrar el usuario';
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-title>Crear cuenta</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <ion-item>
        <ion-label position="stacked">Correo</ion-label>
        <ion-input v-model="email" type="email" placeholder="tu@correo.com" required />
      </ion-item>

      <ion-item>
        <ion-label position="stacked">Contraseña</ion-label>
        <ion-input v-model="password" type="password" placeholder="Mínimo 6 caracteres" required />
      </ion-item>

      <ion-text color="danger" v-if="error">
        <p class="ion-padding-start">{{ error }}</p>
      </ion-text>
      <ion-text color="success" v-if="success">
        <p class="ion-padding-start">Cuenta creada, redirigiendo al login...</p>
      </ion-text>

      <ion-button expand="block" class="ion-margin-top" :disabled="loading" @click="handleRegister">
        {{ loading ? 'Creando...' : 'Registrarme' }}
      </ion-button>

      <ion-button expand="block" fill="clear" router-link="/login">
        ¿Ya tienes cuenta? Inicia sesión
      </ion-button>
    </ion-content>
  </ion-page>
</template>
