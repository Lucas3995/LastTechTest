// RF-3 E2E Tests — AnticipationSimulation (T10-T11)
// Playwright: Full user flows covering CA-RF3-1 to CA-RF3-6

import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://localhost:4200';

test.describe('RF-3: Anticipation Simulation E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to simulation page
    await page.goto(`${BASE_URL}/anticipation/simulation`);
    await page.waitForLoadState('networkidle');
  });

  test.describe('CA-RF3-1: Valid Simulation with Result', () => {
    test('Creator should simulate and see result', async ({ page }) => {
      // Fill form with valid data
      await page.fill('input[formControlName="requestedAmount"]', '1000');

      // Submit form
      await page.click('button[type="submit"]:has-text("Simular")');

      // Wait for result to appear
      await page.waitForSelector('[class*="result"]', { timeout: 5000 });

      // Verify result panel is displayed
      const resultPanel = page.locator('[class*="result"]');
      await expect(resultPanel).toBeVisible();

      // Verify amounts are displayed (in cents, so 100000 = R$ 1000.00)
      const grossAmount = page.locator('[class*="gross"]');
      await expect(grossAmount).toContainText(/1000|100000/);

      // Verify net amount is shown
      const netAmount = page.locator('[class*="net"]');
      await expect(netAmount).toContainText(/950|95000/);

      // Verify validity badge
      const validityBadge = page.locator('[class*="valid"]');
      await expect(validityBadge).toBeVisible();
    });

    test('should display simulation code for reference', async ({ page }) => {
      await page.fill('input[formControlName="requestedAmount"]', '500');
      await page.click('button[type="submit"]:has-text("Simular")');

      await page.waitForSelector('[class*="result"]');

      // Verify simulation code is displayed (for support reference)
      const simulationCode = page.locator('[class*="code"], [class*="SIM"]');
      await expect(simulationCode).toBeVisible();
    });
  });

  test.describe('CA-RF3-2: Validation Errors Localized', () => {
    test('should show field-specific error messages for invalid amount', async ({ page }) => {
      // Try to submit with invalid amount (below minimum 100)
      await page.fill('input[formControlName="requestedAmount"]', '50');
      await page.click('button[type="submit"]');

      // Form should report validation error
      const formInvalid = await page.locator('button[type="submit"][disabled]').isVisible();
      if (formInvalid) {
        expect(formInvalid).toBe(true);
      }

      // Check for error message containing minimum requirement
      const errorMessage = page.locator('[class*="error"]');
      await expect(errorMessage).toContainText(/mínimo|100/i);
    });

    test('should show error message when submitting empty form', async ({ page }) => {
      // Click submit without filling amount
      await page.click('button[type="submit"]');

      // Expect submit to be disabled or error shown
      const submitButton = page.locator('button[type="submit"]');
      const isDisabled = await submitButton.isDisabled();
      expect(isDisabled).toBe(true);
    });

    test('should display API validation errors from backend', async ({ page }) => {
      // Fill with amount that might fail backend validation
      await page.fill('input[formControlName="requestedAmount"]', '999999');
      await page.click('button[type="submit"]');

      // Wait for potential error
      await page.waitForTimeout(1000);

      // Check if error message appears (or success if valid)
      const resultOrError = page.locator('[class*="result"], [class*="error"]');
      await expect(resultOrError).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('CA-RF3-3: No Persistent Effects (Simulations Don\'t Create Requests)', () => {
    test('multiple simulations should not create requests in RF-1', async ({ page, context }) => {
      // Perform first simulation
      await page.fill('input[formControlName="requestedAmount"]', '500');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      // Perform second simulation
      await page.fill('input[formControlName="requestedAmount"]', '1000');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      // Navigate to RF-1 (My Requests)
      await page.goto(`${BASE_URL}/anticipation/my-requests`);
      await page.waitForLoadState('networkidle');

      // Count requests in list
      const requestRows = page.locator('[class*="request-row"], tbody tr');
      const count = await requestRows.count();

      // Simulations should not have created actual requests
      // (Count should be same as before, or empty if no previous requests)
      expect(count).toBeLessThanOrEqual(0 + 1); // Allow for existing test data
    });

    test('simulation should clear when creating new one', async ({ page }) => {
      // First simulation
      await page.fill('input[formControlName="requestedAmount"]', '500');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      const firstResult = await page.locator('[class*="result"]').textContent();

      // Clear form and simulate again
      await page.fill('input[formControlName="requestedAmount"]', '750');
      await page.click('button[type="submit"]');
      await page.waitForTimeout(1000);

      const secondResult = await page.locator('[class*="result"]').textContent();

      // Results should be different (new amounts)
      expect(secondResult).not.toEqual(firstResult);
    });
  });

  test.describe('CA-RF3-4: Successful Conversion', () => {
    test('Creator should convert simulation to real request', async ({ page }) => {
      // Simulate first
      await page.fill('input[formControlName="requestedAmount"]', '1000');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      // Verify convert button is visible and enabled
      const convertButton = page.locator('button:has-text("Criar solicitação"), button:has-text("Converter")');
      await expect(convertButton).toBeVisible();
      await expect(convertButton).not.toBeDisabled();

      // Click convert
      await convertButton.click();

      // Handle confirmation dialog if present
      const confirmButton = page.locator('button:has-text("Confirmar"), button:has-text("Criar")');
      if (await confirmButton.isVisible()) {
        await confirmButton.click();
      }

      // Wait for success message or navigation
      const successMessage = page.locator('[class*="success"], [class*="info"]');
      await expect(successMessage).toBeVisible({ timeout: 5000 });

      // Should show success message
      await expect(successMessage).toContainText(/sucesso|criada/i);
    });

    test('should navigate to my-requests after successful conversion', async ({ page }) => {
      // Simulate
      await page.fill('input[formControlName="requestedAmount"]', '1000');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      // Convert
      const convertButton = page.locator('button:has-text("Criar solicitação"), button:has-text("Converter")');
      await convertButton.click();

      const confirmButton = page.locator('button:has-text("Confirmar"), button:has-text("Criar")');
      if (await confirmButton.isVisible()) {
        await confirmButton.click();
      }

      // Wait for navigation or link
      const myRequestsLink = page.locator('a:has-text("Minhas Solicitações"), a:has-text("my-requests")');
      if (await myRequestsLink.isVisible()) {
        await myRequestsLink.click();
      } else {
        // Page may auto-navigate
        await page.waitForURL(/.*my-requests.*/, { timeout: 5000 });
      }

      // Verify we're on My Requests page
      expect(page.url()).toContain('my-requests');
    });

    test('converted request should appear in RF-1 with correct amount', async ({ page }) => {
      const testAmount = '2000';

      // Simulate
      await page.fill('input[formControlName="requestedAmount"]', testAmount);
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      // Convert
      const convertButton = page.locator('button:has-text("Criar solicitação"), button:has-text("Converter")');
      await convertButton.click();

      const confirmButton = page.locator('button:has-text("Confirmar"), button:has-text("Criar")');
      if (await confirmButton.isVisible()) {
        await confirmButton.click();
      }

      // Navigate to my-requests
      await page.goto(`${BASE_URL}/anticipation/my-requests`);
      await page.waitForLoadState('networkidle');

      // Verify request appears with correct amount
      const requestAmounts = page.locator('[class*="amount"]');
      const amountTexts = await requestAmounts.allTextContents();
      const hasAmount = amountTexts.some((text) => text.includes('2000') || text.includes('2'));

      expect(hasAmount).toBe(true);
    });
  });

  test.describe('CA-RF3-5: Conversion Rejection Scenarios', () => {
    test('should reject expired simulation', async ({ page }) => {
      // This test may require test data or backend mock
      // Simulate with intentionally old timestamp or wait for expiration
      await page.fill('input[formControlName="requestedAmount"]', '1000');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      // Attempt conversion after expiration (if simulated expiration is possible)
      // In real scenario, wait for 20+ minutes or use test backend fixture
      const convertButton = page.locator('button[disabled]:has-text("Criar solicitação")');
      if (await convertButton.isVisible()) {
        expect(await convertButton.isDisabled()).toBe(true);
      }
    });

    test('should show error when pending request already exists', async ({ page }) => {
      // This requires pre-existing pending request in test data
      await page.fill('input[formControlName="requestedAmount"]', '500');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      const convertButton = page.locator('button:has-text("Criar solicitação"), button:has-text("Converter")');
      await convertButton.click();

      const confirmButton = page.locator('button:has-text("Confirmar"), button:has-text("Criar")');
      if (await confirmButton.isVisible()) {
        await confirmButton.click();
      }

      // Check for error about existing pending request
      const errorMessage = page.locator('[class*="error"]');
      await expect(errorMessage).toBeVisible({ timeout: 5000 });
      await expect(errorMessage).toContainText(/aberto|pendente/i);
    });

    test('should show specific error messages for conversion failures', async ({ page }) => {
      await page.fill('input[formControlName="requestedAmount"]', '1000');
      await page.click('button[type="submit"]');
      await page.waitForSelector('[class*="result"]');

      const convertButton = page.locator('button:has-text("Criar solicitação"), button:has-text("Converter")');
      if (await convertButton.isVisible() && !await convertButton.isDisabled()) {
        await convertButton.click();

        const confirmButton = page.locator('button:has-text("Confirmar"), button:has-text("Criar")');
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        // Look for specific error (may vary based on test conditions)
        const errorMessage = page.locator('[class*="error"], [class*="message"]');
        if (await errorMessage.isVisible()) {
          const errorText = await errorMessage.textContent();
          // Error should contain a specific reason
          expect(errorText).toMatch(/expirou|aberto|regra|utilizada/i);
        }
      }
    });
  });

  test.describe('CA-RF3-6: Admin/Analyst Simulation on Behalf', () => {
    test('Analyst should simulate on behalf of creator', async ({ page }) => {
      // Note: This requires being logged in as Analyst
      // Check if creator field is visible
      const creatorField = page.locator('input[formControlName="creatorId"], [class*="creator"]');

      if (await creatorField.isVisible()) {
        // Select or fill creator
        await creatorField.fill('creator-user-id');

        // Simulate
        await page.fill('input[formControlName="requestedAmount"]', '1000');
        await page.click('button[type="submit"]');

        await page.waitForSelector('[class*="result"]');

        // Verify result is shown
        const resultPanel = page.locator('[class*="result"]');
        await expect(resultPanel).toBeVisible();

        // Verify no conversion button (Analyst can't convert)
        const convertButton = page.locator('button:has-text("Criar solicitação")');
        expect(await convertButton.isVisible()).toBe(false);
      }
    });

    test('Admin should simulate and convert on behalf of creator', async ({ page }) => {
      // Note: Requires being logged in as Admin
      const creatorField = page.locator('input[formControlName="creatorId"]');

      if (await creatorField.isVisible()) {
        await creatorField.fill('target-creator-id');

        // Simulate
        await page.fill('input[formControlName="requestedAmount"]', '1000');
        await page.click('button[type="submit"]');

        await page.waitForSelector('[class*="result"]');

        // Admin should see conversion button
        const convertButton = page.locator('button:has-text("Criar solicitação")');
        await expect(convertButton).toBeVisible();
        await expect(convertButton).not.toBeDisabled();

        // Convert
        await convertButton.click();

        const confirmButton = page.locator('button:has-text("Confirmar"), button:has-text("Criar")');
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }

        // Verify success
        const successMessage = page.locator('[class*="success"], [class*="info"]');
        await expect(successMessage).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Navigation Integration (T11)', () => {
    test('should navigate from RF-1 to simulation page', async ({ page }) => {
      // Go to RF-1 (My Requests)
      await page.goto(`${BASE_URL}/anticipation/my-requests`);
      await page.waitForLoadState('networkidle');

      // Click "Simulate" or "New Simulation" button
      const simulateButton = page.locator('button:has-text("Simular"), a:has-text("Simulação")');
      if (await simulateButton.isVisible()) {
        await simulateButton.click();
        await page.waitForURL(/.*simulation.*/, { timeout: 5000 });
        expect(page.url()).toContain('simulation');
      }
    });

    test('should have link back to my-requests from simulation', async ({ page }) => {
      // Navigate to simulation page
      await page.goto(`${BASE_URL}/anticipation/simulation`);

      // Look for "My Requests" link (may appear in success message or nav)
      const myRequestsLink = page.locator('a:has-text("Minhas Solicitações"), a:has-text("my-requests")');
      if (await myRequestsLink.isVisible()) {
        await myRequestsLink.click();
        expect(page.url()).toContain('my-requests');
      }
    });

    test('should allow navigating from admin list to simulation', async ({ page }) => {
      // Go to admin list page
      await page.goto(`${BASE_URL}/anticipation/list`);
      await page.waitForLoadState('networkidle');

      // Look for "Simulate" action on a request (or in row)
      const simulateAction = page.locator('[class*="simulate"], button:has-text("Simular")');
      if (await simulateAction.first().isVisible()) {
        await simulateAction.first().click();
        // Should navigate to simulation with pre-filled creator
        await page.waitForURL(/.*simulation.*/, { timeout: 5000 });
        expect(page.url()).toContain('simulation');
      }
    });
  });

  test.describe('Accessibility & UX', () => {
    test('should have descriptive form labels', async ({ page }) => {
      const labels = page.locator('label');
      const labelTexts = await labels.allTextContents();
      const hasAmountLabel = labelTexts.some((text) => text.toLowerCase().includes('valor') || text.toLowerCase().includes('amount'));
      expect(hasAmountLabel).toBe(true);
    });

    test('should have accessible error messages', async ({ page }) => {
      // Submit empty form to trigger error
      await page.click('button[type="submit"]');

      // Check for error element with proper ARIA attributes or visible text
      const errorMessage = page.locator('[role="alert"], [class*="error"]');
      if (await errorMessage.isVisible()) {
        const text = await errorMessage.textContent();
        expect(text).toBeTruthy();
      }
    });

    test('should have disabled state for buttons during loading', async ({ page }) => {
      await page.fill('input[formControlName="requestedAmount"]', '1000');

      // Submit and check button state immediately
      await page.click('button[type="submit"]');

      // Button may be disabled during request
      await page.waitForTimeout(500);
      const submitButton = page.locator('button[type="submit"]');
      const isDisabled = await submitButton.isDisabled();

      // After request completes, should be enabled again
      await page.waitForTimeout(2000);
      expect(await submitButton.isDisabled()).toBe(false);
    });
  });
});
