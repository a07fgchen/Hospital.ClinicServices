<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="app-header">
    <RouterLink class="brand" to="/department">
      門診掛號系統
    </RouterLink>

    <nav>
      <RouterLink to="/department">門診掛號</RouterLink>
      <RouterLink to="/appointment-query">掛號查詢</RouterLink>
      <RouterLink to="/calling">門診叫號</RouterLink>
    </nav>
  </header>

  <main class="app-main">
    <RouterView />
  </main>
</template>

<style scoped>
.app-header {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 24px;
  width: 100%;
  padding: 16px 24px;
  border-bottom: 1px solid #ddd;
}

.brand {
  color: inherit;
  font-size: 20px;
  font-weight: 600;
  text-decoration: none;
}

nav a {
  color: inherit;
  text-decoration: none;
}

nav a.router-link-active {
  font-weight: 600;
}

.app-main {
  width: 100%;
  max-width: 1280px;
  margin: 0 auto;
  padding: 24px;
}

nav {
  display: flex;
  gap: 16px;
}
</style>
