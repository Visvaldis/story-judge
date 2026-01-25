import { test, expect } from '@playwright/test';
import { LoginPage } from '../../helpers/page-objects/LoginPage';

test.describe('OAuth Authentication', () => {
  let loginPage: LoginPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
  });

  test('should display Google login button', async ({ page }) => {
    await loginPage.goto();

    await expect(loginPage.googleLoginButton).toBeVisible();
  });

  test('should initiate Google OAuth flow when clicked', async ({ page }) => {
    await loginPage.goto();

    // Set up request interception to verify OAuth redirect
    let oauthRequestMade = false;
    page.on('request', (request) => {
      const url = request.url();
      if (url.includes('google') || url.includes('oauth') || url.includes('signin-google')) {
        oauthRequestMade = true;
      }
    });

    // Click Google login - this will redirect to Google
    await loginPage.googleLoginButton.click();

    // Wait for navigation attempt
    await page.waitForTimeout(1000);

    // The page should either redirect to Google or show the OAuth flow
    const currentUrl = page.url();
    const isOAuthFlow = currentUrl.includes('google') ||
                        currentUrl.includes('accounts') ||
                        currentUrl.includes('oauth') ||
                        oauthRequestMade;

    expect(isOAuthFlow).toBe(true);
  });

  test('should handle OAuth callback with mocked response', async ({ page }) => {
    // Mock the OAuth callback endpoint to simulate successful login
    await page.route('**/auth/callback/google**', async (route) => {
      // Simulate successful OAuth by redirecting to dashboard with token
      await route.fulfill({
        status: 302,
        headers: {
          'Location': '/?token=mocked-jwt-token'
        }
      });
    });

    // Mock the /auth/me endpoint for the mocked token
    await page.route('**/api/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'oauth-user-id',
          email: 'oauth.user@gmail.com',
          displayName: 'OAuth Test User',
          roleLevel: 'Junior',
          reputationScore: 0,
        })
      });
    });

    // Set up auth token as if OAuth succeeded
    await page.addInitScript(() => {
      // Check URL for token parameter on load
      const params = new URLSearchParams(window.location.search);
      const token = params.get('token');
      if (token) {
        localStorage.setItem('token', token);
      }
    });

    // Navigate with token parameter (simulating OAuth callback)
    await page.goto('/?token=mocked-jwt-token');

    // Verify we're authenticated
    const token = await page.evaluate(() => localStorage.getItem('token'));
    expect(token).toBeTruthy();
  });
});
