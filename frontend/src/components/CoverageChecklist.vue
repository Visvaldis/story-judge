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
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 1.25rem;
}

.coverage-checklist h3 {
  margin-bottom: 0.25rem;
}

.description {
  color: var(--color-text-secondary);
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
  border-bottom: 1px solid var(--color-border);
  color: var(--color-text-secondary);
}

.checklist-item:last-child {
  border-bottom: none;
}

.checklist-item.checked {
  color: var(--color-success);
}

.checklist-item.checked .icon {
  color: var(--color-success);
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
  color: var(--color-success);
}

.coverage-score .label {
  display: block;
  font-size: 0.75rem;
  font-weight: normal;
  color: var(--color-text-secondary);
}
</style>
