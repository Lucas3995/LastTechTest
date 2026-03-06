import { test, expect } from '@playwright/test';

test.describe('Shell and Home', () => {
  test('should show shell with header and main content', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('header')).toBeVisible();
    await expect(page.locator('nav[aria-label="Main navigation"]')).toBeVisible();
    await expect(page.locator('#main-content')).toBeVisible();
    await expect(page.locator('footer')).toBeVisible();
  });

  test('should show home page content on default route', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'Home', level: 1 })).toBeVisible();
    await expect(page.getByText(/Welcome/)).toBeVisible();
  });

  test('should have accessible Home link in nav', async ({ page }) => {
    await page.goto('/');
    const homeLink = page.getByRole('link', { name: 'Home' });
    await expect(homeLink).toBeVisible();
    await expect(homeLink).toHaveAttribute('href', '/');
  });

  test('Plano 4 ajustes: authenticated user sees logout button; click logs out and redirects to login', async ({
    page,
  }) => {
    const email = process.env.CREATOR_E2E_EMAIL ?? 'creator@example.com';
    const password = process.env.CREATOR_E2E_PASSWORD ?? 'password';

    await page.goto('/');
    await page.getByLabel(/Email corporativo/i).fill(email);
    await page.getByLabel(/Senha/i).fill(password);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(/\/(home|anticipation)/);
    const logoutButton = page.getByRole('button', { name: /Sair|Logout/i });
    await expect(logoutButton).toBeVisible({ timeout: 10000 });

    await logoutButton.click();

    await expect(page).toHaveURL(/\/\?|^\/$/);
    await expect(page.getByRole('button', { name: /Entrar/i }).or(page.getByLabel(/Email corporativo/i))).toBeVisible();
  });
});
