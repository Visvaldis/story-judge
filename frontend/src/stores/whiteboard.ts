import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { ViewMode, Position, Viewport, WhiteboardState } from '../types/whiteboard';

const STORAGE_KEY = 'story-judge-whiteboard';

const DEFAULT_VIEWPORT: Viewport = { x: 0, y: 0, zoom: 1 };

export const useWhiteboardStore = defineStore('whiteboard', () => {
  const positions = ref<Record<string, Position>>({});
  const viewport = ref<Viewport>({ ...DEFAULT_VIEWPORT });
  const viewMode = ref<ViewMode>('grid');

  function loadFromStorage() {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) {
        const state: WhiteboardState = JSON.parse(saved);
        positions.value = state.positions || {};
        viewport.value = state.viewport || { ...DEFAULT_VIEWPORT };
        viewMode.value = state.viewMode || 'grid';
      }
    } catch (e) {
      console.error('Failed to load whiteboard state:', e);
    }
  }

  function saveToStorage() {
    try {
      const state: WhiteboardState = {
        positions: positions.value,
        viewport: viewport.value,
        viewMode: viewMode.value
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch (e) {
      console.error('Failed to save whiteboard state:', e);
    }
  }

  function getPosition(storyId: string, index: number): Position {
    if (positions.value[storyId]) {
      return positions.value[storyId];
    }
    // Auto-layout: 3 columns, spacing 280x200
    return {
      x: (index % 3) * 280 + 50,
      y: Math.floor(index / 3) * 200 + 50
    };
  }

  function savePosition(storyId: string, x: number, y: number) {
    positions.value[storyId] = { x, y };
    saveToStorage();
  }

  function saveViewport(x: number, y: number, zoom: number) {
    viewport.value = { x, y, zoom };
    saveToStorage();
  }

  function toggleViewMode() {
    viewMode.value = viewMode.value === 'grid' ? 'whiteboard' : 'grid';
    saveToStorage();
  }

  function removePosition(storyId: string) {
    delete positions.value[storyId];
    saveToStorage();
  }

  function clearPositions() {
    positions.value = {};
    saveToStorage();
  }

  // Load state on init
  loadFromStorage();

  return {
    positions,
    viewport,
    viewMode,
    getPosition,
    savePosition,
    saveViewport,
    toggleViewMode,
    removePosition,
    clearPositions
  };
});
