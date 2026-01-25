export const testUsers = {
  primary: {
    email: 'test.user.primary@example.com',
    password: 'TestPassword123',
    displayName: 'Primary Test User',
  },
  secondary: {
    email: 'test.user.secondary@example.com',
    password: 'TestPassword123',
    displayName: 'Secondary Test User',
  },
  reviewer: {
    email: 'test.reviewer@example.com',
    password: 'TestPassword123',
    displayName: 'Test Reviewer',
  },
};

export function generateUniqueUser(prefix = 'user') {
  const timestamp = Date.now();
  return {
    email: `test.${prefix}.${timestamp}@example.com`,
    password: 'TestPassword123',
    displayName: `Test ${prefix} ${timestamp}`,
  };
}
