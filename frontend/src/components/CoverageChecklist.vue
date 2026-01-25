<script setup lang="ts">
import type { CoverageItem } from '../types';

defineProps<{
  coverage: CoverageItem[];
  readonly?: boolean;
}>();
</script>

<template>
  <div class="coverage-checklist">
    <h3>Coverage Checklist</h3>
    <p class="description">Signals detected in your story</p>

    <ul class="checklist">
      <li
        v-for="item in coverage"
        :key="item.key"
        class="checklist-item"
        :class="{ checked: item.manualOverride ?? item.detected }"
      >
        <span class="icon">
          {{ (item.manualOverride ?? item.detected) ? '✓' : '○' }}
        </span>
        <span class="label">{{ item.label }}</span>
      </li>
    </ul>

    <div class="coverage-score">
      {{ coverage.filter(c => c.manualOverride ?? c.detected).length }} / {{ coverage.length }}
      <span class="label">coverage</span>
    </div>
  </div>
</template>

<style scoped>
.coverage-checklist {
  background: #1a1a2e;
  border: 1px solid #333;
  border-radius: 8px;
  padding: 1.25rem;
}

.coverage-checklist h3 {
  margin-bottom: 0.25rem;
}

.description {
  color: #888;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.checklist {
  list-style: none;
  padding: 0;
  margin: 0;
}

.checklist-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid #333;
  color: #888;
}

.checklist-item:last-child {
  border-bottom: none;
}

.checklist-item.checked {
  color: #42b883;
}

.checklist-item.checked .icon {
  color: #42b883;
}

.icon {
  font-size: 1rem;
  width: 1.5rem;
}

.coverage-score {
  margin-top: 1rem;
  text-align: center;
  font-size: 1.25rem;
  font-weight: bold;
  color: #42b883;
}

.coverage-score .label {
  display: block;
  font-size: 0.75rem;
  font-weight: normal;
  color: #888;
}
</style>
