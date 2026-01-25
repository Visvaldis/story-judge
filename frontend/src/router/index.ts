import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue')
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue')
  },
  {
    path: '/auth/callback',
    name: 'AuthCallback',
    component: () => import('../views/AuthCallback.vue')
  },
  {
    path: '/explore',
    name: 'Explore',
    component: () => import('../views/Explore.vue')
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/stories/new',
    name: 'NewStory',
    component: () => import('../views/StoryEditor.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/stories/:id/edit',
    name: 'EditStory',
    component: () => import('../views/StoryEditor.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/stories/:id',
    name: 'StoryView',
    component: () => import('../views/StoryView.vue')
  },
  {
    path: '/shared/:shareToken',
    name: 'SharedStory',
    component: () => import('../views/SharedStory.vue')
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore();

  // If route requires auth, check if user is authenticated
  if (to.meta.requiresAuth) {
    if (!authStore.isAuthenticated) {
      // Try to fetch user if we have a token
      await authStore.fetchCurrentUser();

      if (!authStore.isAuthenticated) {
        return next({ name: 'Login', query: { redirect: to.fullPath } });
      }
    }
  }

  next();
});

export default router;
