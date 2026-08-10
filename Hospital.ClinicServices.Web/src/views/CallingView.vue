<template>
  <main class="calling-page">
    <section class="calling-header">
      <span class="page-label">診間作業</span>
      <h1>門診叫號</h1>
      <p>輸入今日門診的場次編號，依序呼叫下一位病患。</p>
    </section>

    <form class="calling-card" @submit.prevent="callNext">
      <div class="form-group">
        <label for="schedule-id">
          門診場次編號 <span class="required">*</span>
        </label>
        <input
          id="schedule-id"
          v-model.number="scheduleId"
          type="text"
          inputmode="numeric"
          pattern="[0-9]*"
          placeholder="例如：1"
          autocomplete="off"
          required
        />
      </div>

      <div class="current-number" aria-live="polite">
        <span>目前叫號</span>
        <strong>
          {{ currentCallingNumber === null ? '尚未叫號' : `${currentCallingNumber} 號` }}
        </strong>
      </div>

      <p v-if="successMessage" class="success-message" role="status">
        {{ successMessage }}
      </p>

      <p v-if="errorMessage" class="error-message" role="alert">
        {{ errorMessage }}
      </p>

      <button class="primary-button" type="submit" :disabled="loading || !scheduleId">
        {{ loading ? '叫號中…' : '叫下一號' }}
      </button>
    </form>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'

type CallNextResponse = {
  currentCallingNumber: number
  message: string
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

const scheduleId = ref<number | null>(null)
const currentCallingNumber = ref<number | null>(null)
const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

async function callNext() {
  if (!scheduleId.value) {
    errorMessage.value = '請輸入門診場次編號'
    return
  }

  loading.value = true
  successMessage.value = ''
  errorMessage.value = ''

  try {
    const response = await fetch(
      `${apiBaseUrl}/api/doctor/${scheduleId.value}/next`,
      {
        method: 'POST',
      }
    )

    if (!response.ok) {
      const problem = await response.json().catch(() => null)

      throw new Error(
        problem?.detail ??
        problem?.title ??
        '叫號失敗，請稍後再試'
      )
    }

    const result: CallNextResponse = await response.json()

    currentCallingNumber.value = result.currentCallingNumber
    successMessage.value = result.message
  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : '叫號失敗，請稍後再試'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.calling-page {
  width: min(760px, 100%);
  margin: 0 auto;
  padding: 40px 24px 64px;
  color: #172033;
}

.calling-header {
  margin-bottom: 16px;
  padding: 28px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
}

.page-label {
  color: #2563eb;
  font-size: 14px;
  font-weight: 700;
}

.calling-header h1 {
  margin-top: 6px;
  color: #172033;
  font-size: 30px;
  font-weight: 700;
}

.calling-header p {
  margin-top: 6px;
  color: #64748b;
}

.calling-card {
  display: grid;
  gap: 20px;
  padding: 28px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgb(15 23 42 / 5%);
}

.form-group {
  display: grid;
  gap: 8px;
  color: #334155;
}

.form-group label {
  font-weight: 600;
}

.form-group input {
  width: 100%;
  padding: 11px 12px;
  color: #172033;
  font: inherit;
  background: #fff;
  border: 1px solid #aaa;
  border-radius: 6px;
}

.form-group input:focus {
  border-color: #3178c6;
  outline: 2px solid rgb(49 120 198 / 20%);
}

.required,
.error-message {
  color: #d93025;
}

.current-number {
  display: grid;
  gap: 8px;
  padding: 28px;
  color: #166534;
  text-align: center;
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 10px;
}

.current-number span {
  font-weight: 600;
}

.current-number strong {
  font-size: 40px;
  line-height: 1.2;
}

.success-message {
  margin: 0;
  padding: 12px 14px;
  color: #18794e;
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 8px;
}

.error-message {
  margin: 0;
  padding: 12px 14px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
}

.primary-button {
  justify-self: start;
  padding: 12px 20px;
  color: #fff;
  font: inherit;
  font-weight: 600;
  background: #2563eb;
  border: 1px solid #2563eb;
  border-radius: 8px;
  cursor: pointer;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

@media (max-width: 640px) {
  .calling-page {
    padding: 24px 16px 48px;
  }

  .calling-header,
  .calling-card {
    padding: 20px;
  }

  .primary-button {
    width: 100%;
  }
}
</style>
