<template>
  <div class="schedule-page">
    <button type="button" class="secondary-button back-button" @click="goBackToDepartment">← 上一步：選擇科別</button>

    <div class="schedule-header">
      <span class="step-label">步驟 2 / 3</span>
      <h1>選擇門診時段</h1>
      <p>請選擇欲掛號的時段</p>
    </div>
    <div class="filters">
      <select v-model="selectedRange" aria-label="選擇週次">
        <option v-for="opt in rangeOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
      <select v-model="selectedShift" aria-label="選擇時段">
        <option :value='0'>全部</option>
        <option v-for="shift in shifts" :key="shift.value" :value="shift.value">
          {{ shift.label }}
        </option>
      </select>
    </div>
    <div v-if="loading" class="loading-state">
      門診排班載入中…
    </div>
    <div v-else-if="errorMessage" class="error-state">
      <p>{{ errorMessage }}</p>

      <button type="button" class="primary-button" @click="fetchSchedules">
        重新載入
      </button>
    </div>
    <div v-else-if="schedules.length === 0" class="empty-state">
      此條件目前沒有可掛號的門診。
    </div>

    <div v-else class="table-wrapper">
      <table class="schedule-table">
        <thead>
          <tr>
            <th>時段</th>
            <th v-for="day in weekDays"> {{ day }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="shift in shifts">
            <td> {{ shift.label }} </td>
            <td v-for="(day, dayIndex) in weekDays">
              <button v-for="schedule in getDoctorsByShiftAndDay(shift.value, dayIndex)" :key="schedule.scheduleId"
                role="button" tabindex="0" @click="goToAppointment(schedule)" @keydown.enter="goToAppointment(schedule)"
                @keydown.space.prevent="goToAppointment(schedule)" class="doctor-card">
                {{ schedule.doctor.name }}
                <span>掛號人數: {{ schedule.currentRegisterCount }}/{{ schedule.maxQuota }}</span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<script setup lang="ts">
import { useAppointmentFlowStore } from "@/stores/appointmentFlow"
import { onMounted, ref, watch } from "vue"
import { useRouter } from "vue-router"

type Schedule = {
  scheduleId: number
  doctorId: number
  doctor: {
    doctorId: number
    name: string
  }
  serviceDate: string
  shift: number
  maxQuota: number
  currentRegisterCount: number
  currentCallingNumber: number
  status: number
}

const loading = ref(true)
const errorMessage = ref('')
const schedules = ref(<Schedule[]>[])
const shifts = [
  { label: '上午', value: 1 },
  { label: '下午', value: 2 },
  { label: '晚上', value: 3 }
]
const rangeOptions = [
  { label: '本周', value: 'thisWeek' },
  { label: '下周', value: 'nextWeek' }
]
const selectedRange = ref('thisWeek')
const selectedShift = ref(0) //0 = 全部 1上午 2下午 3晚上


const weekDays = ['日', '一', '二', '三', '四', '五', '六']

const router = useRouter()
const flow = useAppointmentFlowStore()
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

async function fetchSchedules() {
  loading.value = true
  errorMessage.value = ""

  const weekOffset = selectedRange.value === "nextWeek" ? 1 : 0
  const query = new URLSearchParams({
    weekOffset: String(weekOffset),
    shift: String(selectedShift.value)
  })
  try {
    const response = await fetch(`${apiBaseUrl}/api/schedule/${flow.departmentId}?${query.toString()}`)

    if (!response.ok) {
      const errorData = await response.json().catch(() => null)

      throw new Error(
        errorData?.detail ??
        errorData?.message ??
        '門診排班讀取失敗，請稍後再試'
      )
    }

    const responseJson = await response.json()
    schedules.value = responseJson.data
  } catch (error) {
    errorMessage.value = error instanceof Error
      ? error.message :
      '讀取門診列表資料失敗'
  } finally {
    loading.value = false
  }
}

function goToAppointment(schedule: Schedule) {
  flow.scheduleId = schedule.scheduleId
  flow.doctorId = schedule.doctorId

  router.push({
    name: 'appointment'
  })
}

function goBackToDepartment() {
  router.push({ name: 'department' })
}


function getDoctorsByShiftAndDay(shiftValue: number, dayIndex: number) {
  return schedules.value.filter(s => {
    const date = new Date(s.serviceDate)
    return s.shift === shiftValue && date.getDay() === dayIndex
  })
}
watch(
  [selectedRange, selectedShift],
  fetchSchedules,
)
onMounted(() => {
  if (!flow.departmentId) {
    router.replace('/department')
    return
  }

  fetchSchedules()
})

</script>

<style scoped>
.schedule-page {
  width: min(1280px, 100%);
  margin: 0 auto;
  padding: 40px 24px 64px;
  color: #172033;
}

.back-button {
  margin-bottom: 24px;
}

.schedule-header {
  margin-bottom: 24px;
  padding: 24px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
}

.schedule-header h1 {
  margin-top: 6px;
  color: #0f172a;
  font-size: 30px;
  font-weight: 700;
}

.schedule-header p {
  margin-top: 6px;
  color: #334155;
  font-size: 16px;
  font-weight: 500;
}

.schedule-content {
  display: grid;
  gap: 16px;
}

.table-wrapper {
  width: 100%;
  overflow-x: auto;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgb(15 23 42 / 5%);
}

.schedule-table {
  width: 100%;
  min-width: 920px;
  table-layout: fixed;
  border-collapse: separate;
  border-spacing: 0;
}

.schedule-table th,
.schedule-table td {
  width: 115px;
  padding: 10px;
  color: #334155;
  border-right: 1px solid #e7edf5;
  border-bottom: 1px solid #e7edf5;
  vertical-align: top;
}

.schedule-table th {
  color: #334155;
  text-align: center;
  background: #f7f9fc;
  font-weight: 700;
}

.time-slot {
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 16px;
  background-color: #181818;
}

.time-slot h3 {
  margin: 0 0 12px 0;
  font-size: 20px;
}

.doctor-card {
  display: grid;
  gap: 4px;
  width: 100%;
  margin-bottom: 8px;
  padding: 12px 14px;
  color: #1e3a8a;
  font: inherit;
  text-align: left;
  border-radius: 8px;
  background-color: #eff6ff;
  border: 1px solid #bfdbfe;
  cursor: pointer;
}

.doctor-card:hover {
  background: #dbeafe;
  border-color: #2563eb;
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 20px;
}

.filters select {
  min-width: 180px;
  padding: 10px 12px;
  color: #334155;
  font: inherit;
  background: #fff;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
}

.step-label {
  color: #2563eb;
  font-size: 14px;
  font-weight: 700;
}

.loading-state,
.error-state,
.empty-state {
  padding: 28px;
  color: #64748b;
  text-align: center;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
}

.error-state {
  display: grid;
  gap: 16px;
  color: #b42318;
}

.primary-button,
.secondary-button {
  padding: 10px 16px;
  font: inherit;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
}

.primary-button {
  justify-self: center;
  color: #fff;
  background: #2563eb;
  border: 1px solid #2563eb;
}

.secondary-button {
  color: #334155;
  background: #fff;
  border: 1px solid #cbd5e1;
}
</style>
