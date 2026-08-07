<template>
  <div class="schedule-page">
    <div class="schedule-header">
      <h1>科別名稱</h1>
      <p>請選擇欲掛號的時段</p>
    </div>
    <div class="filters">
      <select v-model="selectedRange">
        <option v-for="opt in rangeOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
      <select v-model="selectedShift">
        <option :value='0'>全部</option>
        <option v-for="shift in shifts" :key="shift.value" :value="shift.value">
          {{ shift.label }}
        </option>
      </select>
    </div>
    <div class="table-wrapper">
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
                掛號人數: {{ schedule.currentRegisterCount }}/{{ schedule.maxQuota }}
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
const errorMessage = ref("")
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

async function fetchSchedules() {
  loading.value = true
  errorMessage.value = ""

  const weekOffset = selectedRange.value === "nextWeek" ? 1 : 0
  const query = new URLSearchParams({
    weekOffset: String(weekOffset),
    shift: String(selectedShift.value)
  })
  try {
    const response = await fetch(`http://localhost:5076/api/schedule/${flow.departmentId}?${query.toString()}`)

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
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
  width: 100%;
  margin: 0 auto;
  padding: 24px;
}

.schedule-header {
  margin-bottom: 24px;
}

.schedule-header h1 {
  font-size: 28px;
  margin-bottom: 8px;
}

.schedule-header p {
  color: #666;
  font-size: 16px;
}

.schedule-content {
  display: grid;
  gap: 16px;
}

.table-wrapper {
  width: 100%;
  overflow-x: auto;
}

.schedule-table {
  width: 100%;
  min-width: 920px;
  table-layout: fixed;
  border-collapse: collapse;
}

.schedule-table th,
.schedule-table td {
  width: 115px;
  padding: 10px;
  border: 1px solid #2f2f2f;
  vertical-align: top;
}

.schedule-table th {
  text-align: center;
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
  padding: 12px 14px;
  border-radius: 8px;
  background-color: #181818;
  border: 1px solid #e0e0e0;
  cursor: pointer;
}

.filters {
  display: flex;
  min-width: 180px;
  padding: 8px 10px;
  gap: 4px;
}

.filters select {
  min-width: 180px;
  padding: 8px;
}
</style>
