<template>
  <main class="appointment-page">
    <button v-if="!appointmentResult" type="button" class="secondary-button back-button" @click="goBackToSchedule">
      ← 上一步：選擇時段
    </button>

    <section class="appointment-card">
      <header class="form-header">
        <span class="step-label">步驟 3 / 3</span>
        <h1>預約掛號</h1>
        <p>請填寫病患資料，完成後送出預約。</p>
      </header>
    </section>

    <section v-if="!appointmentResult" class="schedule-summary">
      <h2>預約資訊</h2>
      <p>排班編號: {{ flow.scheduleId }}</p>
    </section>

    <section v-if="appointmentResult" class="success-card">
      <h1> {{ appointmentResult.message }} </h1>

      <p class="sequence-number">
        看診號碼: {{ appointmentResult.sequenceNumber }}
      </p>

      <p>
        掛號編號: {{ appointmentResult.appointmentId }}
      </p>

      <button type="button" class="primary-button" @click="goBackToDepartment">
        返回門診掛號
      </button>
    </section>

    <form class="appointment-form" v-if="!appointmentResult" @submit.prevent="submitAppointment">
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

      <template v-if="form.visitType === 'first'">
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
      </template>

      <div class="form-group">
        <p v-if="submitError" class="submit-error">
          {{ submitError }}
        </p>

        <button type="submit" class="primary-button" :disabled="isSubmitting">
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
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL
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

type AppointmentResult = {
  appointmentId: number;
  sequenceNumber: number;
  createAt: string;
  message: string;
  status: string;
}

type FormErrors = Partial<Record<keyof AppointmentForm, string>>

const appointmentResult = ref<AppointmentResult | null>(null)

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
      ? `${apiBaseUrl}/api/appointment/register-first-visit`
      : `${apiBaseUrl}/api/appointment/register`

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
    if (!responseData.data || !responseData.data.appointmentId) {
      throw new Error('掛號成功，但無法取得掛號結果')
    }

    appointmentResult.value = responseData.data

  } catch (error) {
    submitError.value =
      error instanceof Error ? error.message : '預約失敗，請稍後再試'
  } finally {
    isSubmitting.value = false
  }
}

function goBackToDepartment() {
  flow.resetFlow()
  router.push({ name: 'department' })
}

function goBackToSchedule() {
  router.push({ name: 'schedule' })
}

onMounted(() => {
  if (!flow.scheduleId) {
    router.replace({
      name: 'schedule',
    })
  }
})

</script>

<style scoped>
.success-card {
  display: grid;
  gap: 16px;
  padding: 32px;
  color: #166534;
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 14px;
  text-align: center;
}

.success-card h1 {
  color: #2e7d32;
}

.sequence-number {
  font-size: 28px;
  font-weight: 700;
}

.success-card button {
  padding: 12px 20px;
  border: 0;
  border-radius: 8px;
  cursor: pointer;
}

.appointment-page {
  width: min(760px, 100%);
  margin: 0 auto;
  padding: 40px 24px 64px;
  color: #172033;
}

.appointment-card {
  margin-bottom: 16px;
  padding: 28px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
}

.form-header {
  color: #64748b;
}

.form-header h1 {
  margin-top: 6px;
  color: #172033;
  font-size: 30px;
  font-weight: 700;
}

.form-header p {
  margin-top: 6px;
}

.step-label {
  color: #2563eb;
  font-size: 14px;
  font-weight: 700;
}

.schedule-summary {
  padding: 16px;
  margin-bottom: 24px;
  border-radius: 8px;
  color: #334155;
  background: #f7f9fc;
  border: 1px solid #dbe3ef;
}

.appointment-form {
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

.form-group input {
  padding: 10px 12px;
  color: #172033;
  background: #fff;
  border: 1px solid #aaa;
  border-radius: 6px;
  font: inherit;
}

.form-group input[type='radio'] {
  accent-color: #2563eb;
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

.primary-button,
.secondary-button {
  padding: 12px 20px;
  font: inherit;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
}

.primary-button {
  color: #fff;
  background: #2563eb;
  border: 1px solid #2563eb;
}

.secondary-button {
  color: #334155;
  background: #fff;
  border: 1px solid #cbd5e1;
}

.back-button {
  margin-bottom: 24px;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
</style>
