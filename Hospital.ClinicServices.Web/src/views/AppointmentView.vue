<template>
  <main class="appointment-page">
    <section class="appointment-card">
      <header class="form-header">
        <h1>預約掛號</h1>
        <p>請填寫病患資料，完成後送出預約。</p>
      </header>
    </section>

    <section class="schedule-summmary">
      <h2>預約資訊</h2>
      <p>排班編號: {{ flow.scheduleId }}</p>
    </section>

    <form class="appointment-form" @submit.prevent="submitAppointment">
      <div class="form-group">
        <label>掛號身分
          <span class="required"> * </span>
        </label>
        <div>
          <input type="radio" v-model="form.visitType" value="first">
          <span>初診</span>
        </div>
        <div>
          <input type="radio" v-model="form.visitType" value="returning">
          <span>複診 </span>
        </div>
        <p v-if="errorMessages.nationalId" class="field-error">
          {{ errorMessages.visitType }}
        </p>
      </div>
      <div class="form-group">
        <label for="national-id">身分證字號
          <span class="required"> * </span>
        </label>
        <input id="national-id" v-model.trim="form.nationalId" type="text" maxlength="10" autocomplete="off"
          placeholder="例如 : A123456789">
        <p v-if="errorMessages.nationalId" class="field-error">
          {{ errorMessages.nationalId }}
        </p>
      </div>

      <div class="form-group">
        <label for="birth-date">
          出生日期
          <span class="required">*</span>
        </label>

        <input id="birth-date" v-model="form.birthDate" type="date" />

        <p v-if="errorMessages.birthDate" class="field-error">
          {{ errorMessages.birthDate }}
        </p>
      </div>

      <div v-if="form.visitType === 'first'">
        <div class="form-group">
          <label for="patient-name">姓名
            <span class="required"> * </span>
          </label>

          <input id="patient-name" v-model.trim="form.patientName" autocomplete="name" placeholder="請輸入姓名"
            type="text" />
          <p v-if="errorMessages.patientName" class="field-error">
            {{ errorMessages.patientName }}
          </p>
        </div>

        <div class="form-group">
          <label for="phone-number">手機號碼
            <span class="required"> * </span>
          </label>

          <input id="phone-number" v-model.trim="form.phoneNumber" type="tel" maxlength="10" autocomplete="tel"
            placeholder="例如：0912345678" />
          <p v-if="errorMessages.phoneNumber" class="field-error">
            {{ errorMessages.phoneNumber }}
          </p>
        </div>
      </div>

      <div class="form-group">
        <p v-if="submitError" class="submit-error">
          {{ submitError }}
        </p>

        <button type="submit" :disabled="isSubmitting">
          {{ isSubmitting ? '預約處理中…' : '確認預約' }}
        </button>
      </div>
    </form>
  </main>
</template>

<script setup lang="ts">
import { useAppointmentFlowStore } from "@/stores/appointmentFlow";
import { onMounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const flow = useAppointmentFlowStore();
const nationalIdPattern = /^[A-Z][12]\d{8}$/
const phoneNumberPattern = /^09\d{8}$/
const errorMessages = reactive<FormErrors>({})
const isSubmitting = ref(false)
const submitError = ref('')

type AppointmentForm = {
  nationalId: string;
  patientName: string | '';
  phoneNumber: string | '';
  birthDate: string | '';
  visitType: 'first' | 'returning';
}

type FormErrors = Partial<Record<keyof AppointmentForm, string>>
const form = reactive<AppointmentForm>({
  nationalId: '',
  patientName: '',
  phoneNumber: '',
  birthDate: '',
  visitType: 'first'
})

function validateForm() {
  errorMessages.nationalId = ''
  errorMessages.patientName = ''
  errorMessages.phoneNumber = ''
  errorMessages.birthDate = ''

  const normalizedNationalId = form.nationalId.toUpperCase()

  if (!nationalIdPattern.test(normalizedNationalId)) {
    errorMessages.nationalId = '請輸入正確的身分證字號'
  }

  if (!form.birthDate) {
    errorMessages.birthDate = '請選擇出生日期'
  }

  if (form.visitType === 'first') {
    if (!form.patientName) {
      errorMessages.patientName = '請輸入姓名'
    }

    if (!phoneNumberPattern.test(form.phoneNumber)) {
      errorMessages.phoneNumber = '請輸入正確的手機號碼'
    }
  }

  form.nationalId = normalizedNationalId

  return !Object.values(errorMessages).some(Boolean)
}

async function submitAppointment() {
  submitError.value = ''

  if (!validateForm()) {
    return
  }

  if (!flow.scheduleId) {
    submitError.value = '找不到預約班表，請重新選擇'
    return
  }

  try {
    isSubmitting.value = true
    const isFirstVisit = form.visitType === 'first'
    const endpoint = isFirstVisit
      ? '/api/appointment/register-first-visit'
      : '/api/appointment/register'

    const requestBody = isFirstVisit
      ? {
        scheduleId: flow.scheduleId,
        nationalId: form.nationalId,
        patientName: form.patientName,
        phoneNumber: form.phoneNumber,
        birthDate: form.birthDate,
      } :
      {
        scheduleId: flow.scheduleId,
        nationalId: form.nationalId,
        birthDate: form.birthDate,
      }

    const response = await fetch(
      endpoint,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody),
      },
    )

    const responseData = await response.json()

    if (!response.ok) {
      throw new Error(
        responseData.detail ??
        responseData.message ??
        '預約失敗'
      )
    }

    console.log('預約成功：', responseData.data)

    alert(
      `預約成功，看診號碼：${responseData.data.sequenceNumber}`,
    )
  } catch (error) {
    submitError.value =
      error instanceof Error ? error.message : '預約失敗，請稍後再試'
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  // if (!flow.scheduleId) {
  //   router.replace({
  //     name: 'schedule',
  //   })
  // }
})
</script>

<style scoped>
.appointment-page {
  width: 100%;
  margin: 0 auto;
  padding: 32px 20px;
}

.appointment-card {
  padding: 32px;
  border: 1px solid #ddd;
  border-radius: 12px;
}

.form-header {
  margin-bottom: 24px;
}

.schedule-summary {
  padding: 16px;
  margin-bottom: 24px;
  border-radius: 8px;
  background: #f5f5f5;
  color: #222;
}

.appointment-form {
  display: grid;
  gap: 20px;
}

.form-group {
  display: grid;
  gap: 8px;
}

.form-group input {
  padding: 10px 12px;
  border: 1px solid #aaa;
  border-radius: 6px;
  font: inherit;
}

.form-group input:focus {
  border-color: #3178c6;
  outline: 2px solid rgb(49 120 198 / 20%);
}

.required,
.field-error,
.submit-error {
  color: #d93025;
}

.field-error {
  margin: 0;
  font-size: 14px;
}

button[type='submit'] {
  padding: 12px 20px;
  border: 0;
  border-radius: 8px;
  cursor: pointer;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
</style>
