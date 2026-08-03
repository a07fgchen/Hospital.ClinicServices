<template>
  <div class="appointment-page">
    <h1>預約門診</h1>

    <div class="card">
      <h2>選擇醫生</h2>
      <select v-model="appointmentData.doctor">
        <option value="">請選擇醫師</option>
        <option value="醫師A">醫師A</option>
        <option value="醫師B">醫師B</option>
      </select>
    </div>

    <div class="card">
      <h2>選擇日期</h2>
      <input type="date" v-model="appointmentData.date">
    </div>

    <div class="card">
      <h2 class="time-list">選擇時段</h2>
      <button v-for="time in timeSlots" :key="time" @click="appointmentData.time = time"
        :class="{ active: appointmentData.time === time }">
        {{ time }}
      </button>
    </div>

    <div class="card">
      <h2>填寫資料</h2>
      <input v-model="appointmentData.patientName" placeholder="請輸入姓名">
      <input v-model="appointmentData.phone" placeholder="請輸入電話">
    </div>

    <button class="submit-btn" @click="submitAppointment">送出預約</button>

    <p v-if="message" class="message">{{ message }} </p>

  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const appointmentData = ref({
  doctor: '',
  date: '',
  time: '',
  patientName: '',
  phone: '',
})

const message = ref('')

const timeSlots = [
  '09:00 - 10:00',
  '10:00 - 11:00',
  '11:00 - 12:00',
  '13:00 - 14:00',
  '14:00 - 15:00',
  '15:00 - 16:00',
]

async function submitAppointment() {
  const { doctor, date, time, patientName, phone } = appointmentData.value

  if (
    !doctor ||
    !date ||
    !time ||
    !patientName ||
    !phone
  ) {
    message.value = '請填寫完整資料'
    return
  }

  const scheduleMap: Record<string, number> = {
    '醫師A|09:00 - 10:00': 1,
    '醫師A|10:00 - 11:00': 2,
    '醫師B|09:00 - 10:00': 3,
  }

  const key = doctor + '|' + time
  const scheduleId = scheduleMap[key]

  if (!scheduleId) {
    message.value = "目前找不到對應排班，請換一個時段"
    return
  }

  const payload = {
    scheduleId,
    patientId: 1,
  }

  try {
    const response = await fetch('http://localhost:5076/api/Appointment/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    const result = await response.json()

    if (!response.ok) {
      message.value = result?.detail ?? '掛號失敗'
      return
    }

    message.value = '掛號成功，號碼：' + result.data.sequenceNumber

  } catch (error) {
    message.value = '無法連線到伺服器，請稍後再試'
    return
  }

}
</script>

<style scoped>
.appointment-page {
  max-width: 700px;
  margin: 40px auto;
  padding: 20px;
  font-family: Arial, Helvetica, sans-serif;
}

.card {
  background-color: #f9f9f9;
  padding: 20px;
  margin-bottom: 20px;
  border-radius: 8px;
}

.time-list {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

button {
  padding: 8px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  cursor: pointer;
}

button.active {
  background-color: #2563eb;
  color: white;
}

.submit-btn {
  width: 100%;
  padding: 10px;
  background-color: #16a34a;
  color: white;
  border: none;
  font-size: 16px;
}

.message {
  margin-top: 12px;
  color: #2563eb;
}
</style>
