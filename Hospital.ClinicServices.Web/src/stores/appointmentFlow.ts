import { defineStore } from 'pinia'

type NullableNumber = number | null

interface AppointmentFlowState {
  departmentId: NullableNumber
  doctorId: NullableNumber
  scheduleId: NullableNumber
  patientName: string
  phoneNumber: string
}

export const useAppointmentFlowStore = defineStore('appointmentFlow', {
  state: (): AppointmentFlowState => ({
    departmentId: null,
    doctorId: null,
    scheduleId: null,
    patientName: '',
    phoneNumber: '',
  }),
  actions: {
    setDepartment(departmentId: number) {
      this.departmentId = departmentId

      // Change of department resets dependent selections.
      this.doctorId = null
      this.scheduleId = null
    },

    setDoctor(doctorId: number) {
      this.doctorId = doctorId

      // Change of doctor resets selected schedule.
      this.scheduleId = null
    },

    setSchedule(scheduleId: number) {
      this.scheduleId = scheduleId
    },

    resetFlow() {
      this.departmentId = null
      this.doctorId = null
      this.scheduleId = null
      this.patientName = ''
      this.phoneNumber = ''
    },
  },
})
