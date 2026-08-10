import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/department',
    },
    {
      path: '/department',
      name: 'department',
      component: () => import('../views/DepartmentView.vue'),
    },
    {
      path: '/schedule',
      name: 'schedule',
      component: () => import('../views/ScheduleView.vue'),
    },
    {
      path: '/appointment',
      name: 'appointment',
      component: () => import('../views/AppointmentView.vue'),
    },
    {
      path: '/appointment-query',
      name: 'appointment-query',
      component: () => import('../views/AppointmentQueryView.vue'),
    },
    {
      path: '/calling',
      name: 'calling',
      component: () => import('../views/CallingView.vue'),
    },
  ],
})

export default router
