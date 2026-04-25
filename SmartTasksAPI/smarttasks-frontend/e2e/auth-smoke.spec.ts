import { expect, test } from '@playwright/test';

test.describe('Auth smoke flow', () => {
    test('registers a new user and navigates to boards after login', async ({ page }) => {
        const timestamp = Date.now();
        const username = `smoke-user-${timestamp}`;
        const email = `smoke-${timestamp}@example.com`;
        const password = 'secret123';

        await page.route('**/api/auth/register', async route => {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify({
                    token: 'register-token',
                    user: {
                        id: '11111111-1111-1111-1111-111111111111',
                        fullName: username,
                        email,
                        createdAtUtc: new Date().toISOString()
                    }
                })
            });
        });

        await page.route('**/api/auth/login', async route => {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify({
                    token: 'login-token',
                    user: {
                        id: '11111111-1111-1111-1111-111111111111',
                        fullName: username,
                        email,
                        createdAtUtc: new Date().toISOString()
                    }
                })
            });
        });

        await page.route('**/api/boards', async route => {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify([])
            });
        });

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
