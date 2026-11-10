<script setup>
import { ref, onMounted, watch } from 'vue';
import api from '../services/api';

const songs = ref([]);
const search = ref('');
const page = ref(1);
const limit = ref(5);
const totalPages = ref(1);
const total = ref(0);

const loading = ref(false);
const errorMsg = ref('');

// Estado del formulario (crear / editar)
const form = ref({ id: null, name: '', singer: '' });
const isEditing = ref(false);

let searchTimeout = null;

async function fetchSongs() {
  loading.value = true;
  errorMsg.value = '';
  try {
    const { data } = await api.get('/songs', {
      params: { page: page.value, limit: limit.value, search: search.value || undefined },
    });
    songs.value = data.data;
    totalPages.value = data.meta.totalPages;
    total.value = data.meta.total;
  } catch (err) {
    errorMsg.value = 'No se pudo cargar el listado de canciones';
  } finally {
    loading.value = false;
  }
}

// Requisito 5: búsqueda por nombre (con pequeño debounce)
watch(search, () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    page.value = 1;
    fetchSongs();
  }, 350);
});

function resetForm() {
  form.value = { id: null, name: '', singer: '' };
  isEditing.value = false;
}

function startEdit(song) {
  form.value = { id: song.id, name: song.name, singer: song.singer };
  isEditing.value = true;
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

async function handleSubmit() {
  errorMsg.value = '';
  try {
    if (isEditing.value) {
      await api.patch(`/songs/${form.value.id}`, {
        name: form.value.name,
        singer: form.value.singer,
      });
    } else {
      await api.post('/songs', {
        name: form.value.name,
        singer: form.value.singer,
      });
    }
    resetForm();
    await fetchSongs();
  } catch (err) {
    errorMsg.value = err.response?.data?.message || 'No se pudo guardar la canción';
  }
}

async function handleDelete(song) {
  if (!confirm(`¿Eliminar "${song.name}" de ${song.singer}?`)) return;
  try {
    await api.delete(`/songs/${song.id}`);
    if (songs.value.length === 1 && page.value > 1) {
      page.value -= 1;
    }
    await fetchSongs();
  } catch (err) {
    errorMsg.value = 'No se pudo eliminar la canción';
  }
}

function goToPage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  page.value = newPage;
  fetchSongs();
}

onMounted(fetchSongs);
</script>

<template>
  <div>
    <section class="card">
      <h2>{{ isEditing ? 'Editar canción' : 'Agregar canción' }}</h2>
      <form class="song-form" @submit.prevent="handleSubmit">
        <input v-model="form.name" type="text" placeholder="Nombre de la canción" required />
        <input v-model="form.singer" type="text" placeholder="Cantante" required />
        <div class="form-actions">
          <button type="submit">{{ isEditing ? 'Guardar cambios' : 'Agregar' }}</button>
          <button v-if="isEditing" type="button" class="secondary" @click="resetForm">Cancelar</button>
        </div>
      </form>
    </section>

    <section class="card">
      <div class="list-header">
        <h2>Canciones ({{ total }})</h2>
        <input v-model="search" type="search" placeholder="Buscar por nombre..." class="search-input" />
      </div>

      <p v-if="errorMsg" class="error">{{ errorMsg }}</p>
      <p v-if="loading">Cargando...</p>

      <ul v-else class="song-list">
        <li v-for="song in songs" :key="song.id" class="song-item">
          <div>
            <strong>{{ song.name }}</strong>
            <span class="singer"> — {{ song.singer }}</span>
          </div>
          <div class="item-actions">
            <button class="secondary" @click="startEdit(song)">Editar</button>
            <button class="danger" @click="handleDelete(song)">Eliminar</button>
          </div>
        </li>
        <li v-if="!songs.length" class="empty">No hay canciones para mostrar.</li>
      </ul>

      <div class="pagination" v-if="totalPages > 1">
        <button :disabled="page === 1" @click="goToPage(page - 1)">Anterior</button>
        <span>Página {{ page }} de {{ totalPages }}</span>
        <button :disabled="page === totalPages" @click="goToPage(page + 1)">Siguiente</button>
      </div>
    </section>
  </div>
</template>

<style scoped>
.card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 2px 10px rgba(0,0,0,0.06);
}

.song-form { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.song-form input {
  flex: 1;
  min-width: 180px;
  padding: 0.6rem 0.7rem;
  border: 1px solid #ccc;
  border-radius: 8px;
}
.form-actions { display: flex; gap: 0.5rem; }

button {
  padding: 0.6rem 1rem;
  border: none;
  border-radius: 8px;
  background: #1f1147;
  color: white;
  cursor: pointer;
}
button.secondary { background: #e4e4e8; color: #1f1147; }
button.danger { background: #c0392b; }
button:disabled { opacity: 0.5; cursor: not-allowed; }

.list-header { display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap; }
.search-input {
  padding: 0.5rem 0.8rem;
  border-radius: 20px;
  border: 1px solid #ccc;
  min-width: 220px;
}

.song-list { list-style: none; padding: 0; margin: 1rem 0 0; }
.song-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid #eee;
}
.singer { color: #666; }
.item-actions { display: flex; gap: 0.5rem; }
.empty { text-align: center; color: #888; padding: 1rem 0; }

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 1.5rem;
}

.error { color: #c0392b; }
</style>
