<template>
  <div class="wrapper">
    <h1>Department</h1>
    <div v-if="loading">Loading...</div>
    <div v-else-if="errorMessage">{{ errorMessage }}</div>
    <div class="card" v-for="department in departments" :key="department.departmentId">
      <h2 @click="() => {
        flow.setDepartment(department.departmentId);
        router.push({name: 'schedule', params: { departmentId: department.departmentId }})
      }">
        {{ department.name }}
      </h2>
    </div>
  </div>
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

async function loadDepartments() {
  loading.value = true
  errorMessage.value = ""
  try {
    const response = await fetch("http://localhost:5076/api/department")
    if (!response.ok) {
      throw new Error("取得部門失敗")
    }

    const responseJson = await response.json()
    departments.value = responseJson.data
  } catch (error) {
    errorMessage.value = "讀取部門資料失敗"
    console.error(error)
  } finally {
    loading.value = false
  }
}
onMounted(() => {
  loadDepartments()
})

</script>

<style scoped>

.card {
  border: 1px solid #ccc;
  padding: 10px;
  margin-bottom: 20px;
  cursor: pointer;
}
.card:hover {
  background-color: #064194;
}
</style>
