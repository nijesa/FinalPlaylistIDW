<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonButtons, IonButton, IonIcon,
  IonContent, IonSearchbar, IonList, IonItem, IonLabel,
  IonItemSliding, IonItemOptions, IonItemOption,
  IonInfiniteScroll, IonInfiniteScrollContent,
  IonModal, IonInput,
  IonFab, IonFabButton,
  alertController, toastController,
} from '@ionic/vue';
import { logOutOutline, createOutline, trashOutline, addOutline } from 'ionicons/icons';
import api from '../services/api';
import { useAuthStore } from '../stores/auth';

const auth = useAuthStore();
const router = useRouter();

// --- Estado del listado ---
const songs = ref([]);
const search = ref('');
const page = ref(1);
const limit = ref(10);
const totalPages = ref(1);
const loading = ref(false);
const isInfiniteDisabled = ref(false);

// --- Estado del modal (crear/editar) ---
const isModalOpen = ref(false);
const isEditing = ref(false);
const form = ref({ id: null, name: '', singer: '' });

let searchTimeout;

// Requisito 2: listar consumiendo la API.
// `reset = true` reemplaza el listado (nueva búsqueda o primera carga).
// `reset = false` agrega resultados al final (scroll infinito).
async function fetchSongs(reset = false) {
  loading.value = true;
  try {
    const { data } = await api.get('/songs', {
      params: { page: page.value, limit: limit.value, search: search.value || undefined },
    });
    songs.value = reset ? data.data : [...songs.value, ...data.data];
    totalPages.value = data.meta.totalPages;
    isInfiniteDisabled.value = page.value >= totalPages.value;
  } catch (err) {
    await showToast('No se pudo cargar el listado de canciones', 'danger');
  } finally {
    loading.value = false;
  }
}

// Requisito 4: paginación con scroll infinito.
// Ionic llama este handler cuando el usuario llega al final de la lista.
async function loadMore(event) {
  if (page.value >= totalPages.value) {
    isInfiniteDisabled.value = true;
    event.target.complete();
    return;
  }
  page.value += 1;
  await fetchSongs(false);
  event.target.complete();
}

// Requisito 3: búsqueda por nombre con ion-searchbar (con debounce)
function handleSearchInput(event) {
  search.value = event.detail.value ?? '';
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    page.value = 1;
    isInfiniteDisabled.value = false;
    fetchSongs(true);
  }, 350);
}

function resetForm() {
  form.value = { id: null, name: '', singer: '' };
  isEditing.value = false;
}

// Requisito 5: crear/editar con ion-modal
function openCreateModal() {
  resetForm();
  isModalOpen.value = true;
}

function openEditModal(song) {
  form.value = { id: song.id, name: song.name, singer: song.singer };
  isEditing.value = true;
  isModalOpen.value = true;
}

function closeModal() {
  isModalOpen.value = false;
  resetForm();
}

async function handleSubmit() {
  if (!form.value.name.trim() || !form.value.singer.trim()) return;
  try {
    if (isEditing.value && form.value.id) {
      await api.patch(`/songs/${form.value.id}`, { name: form.value.name, singer: form.value.singer });
      await showToast('Canción actualizada', 'success');
    } else {
      await api.post('/songs', { name: form.value.name, singer: form.value.singer });
      await showToast('Canción agregada', 'success');
    }
    closeModal();
    page.value = 1;
    isInfiniteDisabled.value = false;
    await fetchSongs(true);
  } catch (err) {
    await showToast(err.response?.data?.message || 'No se pudo guardar la canción', 'danger');
  }
}

// Requisito 5: eliminar con ion-alert
async function confirmDelete(song) {
  const alert = await alertController.create({
    header: 'Eliminar canción',
    message: `¿Eliminar "${song.name}" de ${song.singer}?`,
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Eliminar',
        role: 'destructive',
        handler: async () => {
          await api.delete(`/songs/${song.id}`);
          page.value = 1;
          isInfiniteDisabled.value = false;
          await fetchSongs(true);
          await showToast('Canción eliminada', 'success');
        },
      },
    ],
  });
  await alert.present();
}

async function showToast(message, color) {
  const toast = await toastController.create({ message, duration: 2000, color, position: 'bottom' });
  await toast.present();
}

async function handleLogout() {
  await auth.logout();
  router.push({ name: 'login' });
}

onMounted(() => fetchSongs(true));
</script>

<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-title>Playlists</ion-title>
        <ion-buttons slot="end">
          <ion-button @click="handleLogout">
            <ion-icon :icon="logOutOutline" slot="icon-only" />
          </ion-button>
        </ion-buttons>
      </ion-toolbar>
      <ion-toolbar>
        <ion-searchbar
          placeholder="Buscar por nombre..."
          :value="search"
          @ionInput="handleSearchInput"
        />
      </ion-toolbar>
    </ion-header>

    <ion-content>
      <ion-list>
        <ion-item-sliding v-for="song in songs" :key="song.id">
          <ion-item>
            <ion-label>
              <h2>{{ song.name }}</h2>
              <p>{{ song.singer }}</p>
            </ion-label>
          </ion-item>
          <ion-item-options side="end">
            <ion-item-option color="primary" @click="openEditModal(song)">
              <ion-icon :icon="createOutline" slot="icon-only" />
            </ion-item-option>
            <ion-item-option color="danger" @click="confirmDelete(song)">
              <ion-icon :icon="trashOutline" slot="icon-only" />
            </ion-item-option>
          </ion-item-options>
        </ion-item-sliding>

        <ion-item v-if="!loading && !songs.length">
          <ion-label class="ion-text-center">No hay canciones para mostrar.</ion-label>
        </ion-item>
      </ion-list>

      <!-- Requisito 4: scroll infinito -->
      <ion-infinite-scroll @ionInfinite="loadMore" :disabled="isInfiniteDisabled">
        <ion-infinite-scroll-content
          loading-spinner="bubbles"
          loading-text="Cargando más canciones..."
        />
      </ion-infinite-scroll>

      <ion-fab vertical="bottom" horizontal="end" slot="fixed">
        <ion-fab-button @click="openCreateModal">
          <ion-icon :icon="addOutline" />
        </ion-fab-button>
      </ion-fab>

      <!-- Requisito 5: modal para crear/editar -->
      <ion-modal :is-open="isModalOpen" @didDismiss="closeModal">
        <ion-header>
          <ion-toolbar>
            <ion-title>{{ isEditing ? 'Editar canción' : 'Agregar canción' }}</ion-title>
            <ion-buttons slot="end">
              <ion-button @click="closeModal">Cerrar</ion-button>
            </ion-buttons>
          </ion-toolbar>
        </ion-header>
        <ion-content class="ion-padding">
          <ion-item>
            <ion-label position="stacked">Nombre</ion-label>
            <ion-input v-model="form.name" placeholder="Nombre de la canción" />
          </ion-item>
          <ion-item>
            <ion-label position="stacked">Cantante</ion-label>
            <ion-input v-model="form.singer" placeholder="Cantante" />
          </ion-item>

          <ion-button expand="block" class="ion-margin-top" @click="handleSubmit">
            {{ isEditing ? 'Guardar cambios' : 'Agregar' }}
          </ion-button>
        </ion-content>
      </ion-modal>
    </ion-content>
  </ion-page>
</template>
