<template>
  <main class="page-shell">
    <header class="page-header">
      <span class="step-label">步驟 1 / 3</span>
      <h1>選擇掛號科別</h1>
      <p>請選擇您要預約的門診科別。</p>
    </header>

    <section v-if="loading" class="state-card">科別載入中…</section>
    <section v-else-if="errorMessage" class="state-card error-state">
      <p>{{ errorMessage }}</p>

      <button type="button" class="primary-button" @click="loadDepartments">
        重新載入
      </button>
    </section>

    <section v-else-if="departments.length === 0" class="state-card">
      目前沒有可供掛號的科別。
    </section>
    <section v-else class="department-grid" aria-label="門診科別">
      <button class="department-card" type="button" v-for="department in departments"
        :key="department.departmentId" @click="selectDepartment(department.departmentId)">
        <span class="department-icon" aria-hidden="true">＋</span>
        <span class="department-name">
          {{ department.name }}
        </span>
        <span class="card-action">查看排班 →</span>
      </button>
    </section>
  </main>
</template>

<script setup lang="ts">
import { useAppointmentFlowStore } from "@/stores/appointmentFlow"
import { onMounted, ref } from "vue"
import { useRouter } from "vue-router"

interface Department {
  departmentId: number
  name: string
}

const departments = ref<Department[]>([])
const loading = ref(true)
const errorMessage = ref("")
const router = useRouter()
const flow = useAppointmentFlowStore()
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

async function loadDepartments() {
  loading.value = true
  errorMessage.value = ""
  try {
    const response = await fetch(`${apiBaseUrl}/api/department`)
    if (!response.ok) {
      throw new Error("科別資料讀取失敗，請稍後再試")
    }

    const responseJson = await response.json()
    departments.value = responseJson.data

  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : '讀取科別資料失敗'
  } finally {
    loading.value = false
  }
}

function selectDepartment(departmentId: number) {
  flow.setDepartment(departmentId)
  router.push({ name: 'schedule' })
}

onMounted(() => {
  loadDepartments()
})

</script>

<style scoped>
.page-shell {
  width: min(1120px, 100%);
  margin: 0 auto;
  padding: 40px 24px 64px;
  color: #172033;
}

.page-header {
  margin-bottom: 28px;
}

.step-label {
  color: #2563eb;
  font-size: 14px;
  font-weight: 700;
}

.page-header h1 {
  margin-top: 6px;
  color: #172033;
  font-size: 30px;
  font-weight: 700;
}

.page-header p {
  margin-top: 6px;
  color: #64748b;
}

.department-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.department-card {
  display: grid;
  gap: 12px;
  min-height: 160px;
  padding: 22px;
  color: #172033;
  text-align: left;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
  cursor: pointer;
  transition: border-color 0.2s, box-shadow 0.2s, transform 0.2s;
}

.department-card:hover {
  border-color: #2563eb;
  box-shadow: 0 10px 24px rgb(37 99 235 / 12%);
  transform: translateY(-2px);
}

.department-card:focus-visible {
  outline: 3px solid rgb(37 99 235 / 25%);
  outline-offset: 2px;
}

.department-icon {
  display: grid;
  width: 36px;
  height: 36px;
  color: #fff;
  font-size: 24px;
  line-height: 1;
  background: #2563eb;
  border-radius: 10px;
  place-items: center;
}

.department-name {
  font-size: 20px;
  font-weight: 700;
}

.card-action {
  color: #2563eb;
  font-size: 14px;
  font-weight: 600;
}

.state-card {
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

.primary-button {
  justify-self: center;
  padding: 10px 18px;
  color: #fff;
  font: inherit;
  font-weight: 600;
  background: #2563eb;
  border: 0;
  border-radius: 8px;
  cursor: pointer;
}
</style>
