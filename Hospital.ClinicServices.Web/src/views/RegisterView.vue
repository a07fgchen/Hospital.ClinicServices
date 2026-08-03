<template>
  <div class="register-page">
    <h1>病患資料註冊</h1>

    <div class="card">
      <div class="field">
        <label>身分證字號</label>
        <input v-model="formData.nationalId" placeholder="請輸入身份證字號">
      </div>

      <div class="field">
        <label>姓名</label>
        <input v-model="formData.name" placeholder="請輸入姓名">
      </div>

      <div class="field">
        <label>電話號碼</label>
        <input v-model="formData.phoneNumber" placeholder="請輸入電話號碼">
      </div>

      <div class="field">
        <label>生日</label>
        <input type="date" v-model="formData.birthDate" placeholder="請輸入生日">
      </div>
    </div>

    <button class="submit-btn" @click="submitRegister">
      送出資料
    </button>

    <p v-if="message" class="message"> {{ message }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const formData = ref({
  nationalId: 'Q123456789',
  name: 'John Doe',
  birthDate: '',
  phoneNumber: '096666666',
})

const message = ref('')

async function submitRegister() {
  const { nationalId, name, phoneNumber, birthDate } = formData.value
  if (
    !nationalId ||
    !name ||
    !phoneNumber ||
    !birthDate
  ) {
    message.value = '請填寫完整資料'
    return
  }

  try {
    const response = await fetch('http://localhost:5076/api/Patient/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ nationalId, name, birthDate, phoneNumber })
    })

    const result = await response.json()

    if (!response.ok) {
      message.value = result.message || '註冊失敗'
      return
    }

    const patientId = result.data.patientId
    message.value = `註冊成功，病歷號碼：${patientId}`

    router.push({ name: 'appointment', query: { patientId } })
  } catch (error) {
    message.value = '無法連線到伺服器，請稍後再試'
    return
  }
}
</script>

<style scoped>
.register-page {
  max-width: 500px;
  margin: 40px;
  padding: 20px;
  font-family: Arial, Helvetica, sans-serif;
}

.card {
  background-color: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.field {
  margin-bottom: 15px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.submit-btn {
  width: 100%;
  padding: 10px;
  background-color: #2563eb;
  color: white;
  border: none;
  font-size: 16px;
  border-radius: 4px;
  cursor: pointer;
}

.message {
  margin-top: 12px;
  color: #16a34a;
}
</style>
