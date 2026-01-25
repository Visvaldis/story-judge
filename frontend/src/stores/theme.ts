import { defineStore } from 'pinia';
import { ref } from 'vue';

export type Theme = 'light' | 'dark';

export const useThemeStore = defineStore('theme', () => {
  const theme = ref<Theme>(getInitialTheme());

  function getInitialTheme(): Theme {
    const saved = localStorage.getItem('theme') as Theme;
    if (saved === 'light' || saved === 'dark') {
      return saved;
    }
    return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
  }

  function setTheme(newTheme: Theme) {
    theme.value = newTheme;
    localStorage.setItem('theme', newTheme);
    applyTheme(newTheme);
  }

  function toggleTheme() {
    setTheme(theme.value === 'dark' ? 'light' : 'dark');
  }

  function applyTheme(t: Theme) {
    document.documentElement.setAttribute('data-theme', t);
  }

  // Apply theme on init
  applyTheme(theme.value);

  // Watch for system preference changes
  window.matchMedia('(prefers-color-scheme: light)').addEventListener('change', (e) => {
    if (!localStorage.getItem('theme')) {
      setTheme(e.matches ? 'light' : 'dark');
    }
  });

  return {
    theme,
    setTheme,
    toggleTheme
  };
});
