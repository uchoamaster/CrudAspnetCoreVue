<script setup>
import { computed, onMounted, reactive, ref } from 'vue'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100'

const items = ref([])
const loading = ref(false)
const message = ref('')
const messageType = ref('')
const editingId = ref(null)

const form = reactive({
  name: '',
  price: '',
  stock: ''
})

const submitLabel = computed(() => (editingId.value ? 'Salvar alteracoes' : 'Adicionar produto'))
const isEditing = computed(() => editingId.value !== null)

const showMessage = (text, type) => {
  message.value = text
  messageType.value = type
}

const clearMessage = () => {
  message.value = ''
  messageType.value = ''
}

const resetForm = () => {
  form.name = ''
  form.price = ''
  form.stock = ''
  editingId.value = null
}

const mapPayload = () => ({
  name: form.name.trim(),
  price: Number(form.price),
  stock: Number(form.stock)
})

const validateForm = () => {
  if (!form.name.trim()) {
    showMessage('Informe o nome do produto.', 'error')
    return false
  }

  if (form.price === '' || Number(form.price) < 0) {
    showMessage('Preco deve ser maior ou igual a zero.', 'error')
    return false
  }

  if (form.stock === '' || Number(form.stock) < 0) {
    showMessage('Estoque deve ser maior ou igual a zero.', 'error')
    return false
  }

  return true
}

const loadProducts = async () => {
  loading.value = true
  clearMessage()

  try {
    const response = await fetch(`${API_BASE_URL}/api/products`)
    if (!response.ok) {
      throw new Error('Falha ao consultar produtos')
    }

    items.value = await response.json()
  } catch (error) {
    showMessage(error.message, 'error')
  } finally {
    loading.value = false
  }
}

const saveProduct = async () => {
  if (!validateForm()) {
    return
  }

  clearMessage()
  const payload = mapPayload()
  const isUpdate = editingId.value !== null
  const targetUrl = isUpdate
    ? `${API_BASE_URL}/api/products/${editingId.value}`
    : `${API_BASE_URL}/api/products`

  try {
    const response = await fetch(targetUrl, {
      method: isUpdate ? 'PUT' : 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    })

    if (!response.ok) {
      const apiError = await response.text()
      throw new Error(apiError || 'Erro ao salvar produto')
    }

    showMessage(isUpdate ? 'Produto atualizado com sucesso.' : 'Produto criado com sucesso.', 'success')
    resetForm()
    await loadProducts()
  } catch (error) {
    showMessage(error.message, 'error')
  }
}

const startEdit = (item) => {
  clearMessage()
  editingId.value = item.id
  form.name = item.name
  form.price = item.price
  form.stock = item.stock
}

const removeProduct = async (id) => {
  clearMessage()

  try {
    const response = await fetch(`${API_BASE_URL}/api/products/${id}`, {
      method: 'DELETE'
    })

    if (!response.ok) {
      throw new Error('Erro ao remover produto')
    }

    if (editingId.value === id) {
      resetForm()
    }

    showMessage('Produto removido com sucesso.', 'success')
    await loadProducts()
  } catch (error) {
    showMessage(error.message, 'error')
  }
}

onMounted(loadProducts)
</script>

<template>
  <main class="page">
    <section class="panel">
      <h1>CRUD ASP.NET Core + Vue</h1>
      <p class="subtitle">Gerencie produtos em uma API .NET 9 e interface Vue 3.</p>

      <form class="form-grid" @submit.prevent="saveProduct">
        <label>
          Nome
          <input v-model="form.name" type="text" placeholder="Ex: Monitor Ultrawide" />
        </label>

        <label>
          Preco (R$)
          <input v-model.number="form.price" type="number" min="0" step="0.01" placeholder="0.00" />
        </label>

        <label>
          Estoque
          <input v-model.number="form.stock" type="number" min="0" step="1" placeholder="0" />
        </label>

        <div class="actions">
          <button type="submit">{{ submitLabel }}</button>
          <button type="button" class="ghost" @click="resetForm" v-if="isEditing">Cancelar</button>
          <button type="button" class="ghost" @click="loadProducts">Atualizar lista</button>
        </div>
      </form>

      <p v-if="message" :class="['notice', messageType]">{{ message }}</p>

      <div v-if="loading" class="notice info">Carregando produtos...</div>

      <table v-else class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nome</th>
            <th>Preco</th>
            <th>Estoque</th>
            <th>Acoes</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.name }}</td>
            <td>R$ {{ Number(item.price).toFixed(2) }}</td>
            <td>{{ item.stock }}</td>
            <td class="row-actions">
              <button type="button" class="small" @click="startEdit(item)">Editar</button>
              <button type="button" class="small danger" @click="removeProduct(item.id)">Excluir</button>
            </td>
          </tr>
          <tr v-if="!items.length">
            <td colspan="5" class="empty">Nenhum produto cadastrado.</td>
          </tr>
        </tbody>
      </table>
    </section>
  </main>
</template>
