<template>
  <main class="query-page">
    <button type="button" class="secondary-button back-button" @click="goBack">
      ← 返回選擇科別
    </button>

    <section class="query-header">
      <span class="page-label">門診查詢</span>
      <h1>查詢我的門診</h1>
      <p>請輸入掛號時使用的身分證字號與生日。</p>
    </section>

    <form class="query-form" @submit.prevent="queryAppointments">
      <div class="form-group">
        <label for="query-national-id">
          身分證字號 <span class="required">*</span>
        </label>
        <input id="query-national-id" v-model.trim="form.nationalId" type="text" maxlength="10" autocomplete="off"
          placeholder="例如：A123456789" required />
      </div>

      <div class="form-group">
        <label for="query-birth-date">
          出生日期 <span class="required">*</span>
        </label>
        <input id="query-birth-date" v-model="form.birthDate" type="date" required />
      </div>

      <p v-if="errorMessage" class="submit-error" role="alert">
        {{ errorMessage }}
      </p>

      <button type="submit" class="primary-button" :disabled="loading">
        {{ loading ? '查詢中…' : '查詢門診' }}
      </button>
    </form>

    <section v-if="searched && appointments.length === 0 && !errorMessage" class="state-card">
      查無符合條件的門診預約。
    </section>

    <section v-else-if="appointments.length > 0" class="results-section">
      <header class="results-header">
        <h2>門診預約</h2>
        <span>共 {{ appointments.length }} 筆</span>
      </header>

      <div class="appointment-list">
        <article v-for="appointment in appointments" :key="appointment.appointmentId" class="appointment-card">
          <header class="card-header">
            <div>
              <span class="date-label">{{ appointment.serviceDate.slice(0, 10) }}</span>
              <h3>{{ appointment.doctorName }}</h3>
            </div>
            <span class="status-badge">{{ getStatusLabel(appointment.appointmentStatus) }}</span>
          </header>

          <dl class="appointment-details">
            <div>
              <dt>看診時段</dt>
              <dd>{{ getShiftLabel(appointment.shift) }}</dd>
            </div>
            <div>
              <dt>診間</dt>
              <dd>{{ appointment.roomNumber || '尚未安排' }}</dd>
            </div>
            <div>
              <dt>您的號碼</dt>
              <dd class="sequence-number">{{ appointment.sequenceNumber }} 號</dd>
            </div>
          </dl>

          <div v-if="appointment.isToday" class="calling-panel" aria-live="polite">
            <span>目前叫號</span>
            <strong>{{ appointment.currentCallingNumber ?? 0 }} 號</strong>
          </div>
          <p v-else class="calling-notice">看診當日才會顯示目前叫號。</p>
        </article>
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
import { onBeforeUnmount, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import * as signalR from '@microsoft/signalr'

type AppointmentQueryResult = {
  appointmentId: number
  scheduleId: number
  sequenceNumber: number
  serviceDate: string
  shift: number
  doctorName: string
  roomNumber: string
  isToday: boolean
  currentCallingNumber: number | null
  appointmentStatus: number
}

const form = reactive({
  nationalId: '',
  birthDate: ''
})

const router = useRouter()
const loading = ref(false)
const searched = ref(false)
const errorMessage = ref('')
const appointments = ref<AppointmentQueryResult[]>([])
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${apiBaseUrl}/hub/queue`)
  .withAutomaticReconnect()
  .build()

connection.onreconnected(async () => {
  try {
    await connectToCallingUpdates()
  } catch (error) {
    console.error('重新加入叫號群組失敗', error)
  }
})

connection.on('ReceiveNumberUpdate', (update: {
  scheduleId: number
  currentCallingNumber: number
  roomNumber: string
}) => {
  const appointment = appointments.value.find(
    item => item.scheduleId === update.scheduleId
  )

  if (!appointment) {
    return
  }

  appointment.currentCallingNumber = update.currentCallingNumber
  appointment.roomNumber = update.roomNumber
}
)

onBeforeUnmount(async () => {
  await connection.stop()
})

async function connectToCallingUpdates() {
  if (
    connection.state === signalR.HubConnectionState.Disconnected
  ) {
    await connection.start()
  }
  const todayScheduleIds = [
    ...new Set(
      appointments.value
        .filter(appointment => appointment.isToday)
        .map(appointment => appointment.scheduleId)
    )
  ]

  await Promise.all(
    todayScheduleIds.map(scheduleId => connection.invoke(
      'JoinClinicQueueGroup',
      scheduleId
    ))
  )
}

async function queryAppointments() {
  loading.value = true
  searched.value = false
  errorMessage.value = ''
  appointments.value = []

  try {
    const response = await fetch(`${apiBaseUrl}/api/appointment/query`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        nationalId: form.nationalId.trim().toUpperCase(),
        birthDate: form.birthDate
      })
    })

    if (!response.ok) {
      const errorData = await response.json().catch(() => null)

      throw new Error(
        errorData?.detail ??
        errorData?.message ??
        '查詢失敗，請稍後再試'
      )
    }

    const result = await response.json()
    appointments.value = result.data ?? []
    searched.value = true

    await connectToCallingUpdates()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : '查詢失敗'
  } finally {
    loading.value = false
  }
}

function getShiftLabel(shift: number) {
  return ({ 1: '上午', 2: '下午', 3: '晚上' } as Record<number, string>)[shift] ?? '未指定'
}

function getStatusLabel(status: number) {
  return ({ 0: '已掛號', 1: '已取消', 2: '已看診' } as Record<number, string>)[status] ?? '未知狀態'
}

function goBack() {
  router.push({ name: 'department' })
}
</script>

<style scoped>
.query-page {
  width: min(900px, 100%);
  margin: 0 auto;
  padding: 40px 24px 64px;
  color: #172033;
}

.back-button {
  margin-bottom: 24px;
}

.query-header {
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

.query-header h1 {
  margin-top: 6px;
  color: #172033;
  font-size: 30px;
  font-weight: 700;
}

.query-header p {
  margin-top: 6px;
  color: #64748b;
}

.query-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 20px;
  margin-bottom: 24px;
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
.submit-error {
  color: #d93025;
}

.submit-error {
  grid-column: 1 / -1;
  margin: 0;
}

.primary-button,
.secondary-button {
  padding: 12px 20px;
  font: inherit;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
}

.primary-button {
  grid-column: 1 / -1;
  justify-self: start;
  color: #fff;
  background: #2563eb;
  border: 1px solid #2563eb;
}

.secondary-button {
  color: #334155;
  background: #fff;
  border: 1px solid #cbd5e1;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.state-card {
  padding: 28px;
  color: #64748b;
  text-align: center;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
}

.results-section {
  display: grid;
  gap: 16px;
}

.results-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.results-header h2 {
  font-size: 22px;
}

.results-header span {
  color: #64748b;
}

.appointment-list {
  display: grid;
  gap: 16px;
}

.appointment-card {
  padding: 24px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgb(15 23 42 / 5%);
}

.card-header {
  display: flex;
  gap: 16px;
  align-items: flex-start;
  justify-content: space-between;
  padding-bottom: 16px;
  border-bottom: 1px solid #e7edf5;
}

.date-label {
  color: #64748b;
  font-size: 14px;
}

.card-header h3 {
  margin-top: 4px;
  color: #172033;
  font-size: 22px;
}

.status-badge {
  flex: 0 0 auto;
  padding: 5px 10px;
  color: #166534;
  font-size: 14px;
  font-weight: 700;
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 999px;
}

.appointment-details {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
  margin: 20px 0;
}

.appointment-details div {
  display: grid;
  gap: 4px;
}

.appointment-details dt {
  color: #64748b;
  font-size: 14px;
}

.appointment-details dd {
  margin: 0;
  color: #334155;
  font-weight: 600;
}

.appointment-details .sequence-number {
  color: #1d4ed8;
  font-size: 20px;
}

.calling-panel {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 18px;
  color: #166534;
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 10px;
}

.calling-panel strong {
  font-size: 24px;
}

.calling-notice {
  margin: 0;
  padding: 14px 16px;
  color: #64748b;
  background: #f7f9fc;
  border: 1px solid #dbe3ef;
  border-radius: 8px;
}

@media (max-width: 640px) {
  .query-page {
    padding: 24px 16px 48px;
  }

  .query-form,
  .appointment-details {
    grid-template-columns: 1fr;
  }

  .primary-button {
    width: 100%;
  }
}
</style>
