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
});
