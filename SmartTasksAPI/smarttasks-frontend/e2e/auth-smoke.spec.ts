import { expect, test } from '@playwright/test';

test.describe('Auth smoke flow', () => {
    test('registers a new user and navigates to boards after login', async ({ page }) => {
        const timestamp = Date.now();
        const username = `smoke-user-${timestamp}`;
        const email = `smoke-${timestamp}@example.com`;
        const password = 'secret123';

        await page.goto('/register');

        await page.fill('#username', username);
        await page.fill('#email', email);
        await page.fill('#password', password);
        await page.fill('#confirmPassword', password);
        await page.getByRole('button', { name: 'Register' }).click();

        await expect(page.getByText('Account created successfully.')).toBeVisible();
        await page.waitForURL('**/login');

        await page.fill('#username', username);
        await page.fill('#password', password);
        await page.getByRole('button', { name: 'Login' }).click();

        await expect(page.getByText('Login successful.')).toBeVisible();
        await page.waitForURL('**/boards');
        await expect(page.getByRole('heading', { name: 'Boards' })).toBeVisible();
    });
});
