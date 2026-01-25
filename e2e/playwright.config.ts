import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,  // Run tests sequentially to avoid rate limits
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,  // Single worker to avoid rate limits
  reporter: [
    ['html', { open: 'never' }],
    ['list']
  ],
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],

  globalSetup: './global-setup.ts',
  globalTeardown: './global-teardown.ts',

  // Skip webServer config - expect services to be running manually or via CI
  // Run: docker-compose -f docker-compose.test.yml up -d
  // Run: cd backend && dotnet run --project src/StoryJudge.Api
  // Run: cd frontend && npm run dev
});
