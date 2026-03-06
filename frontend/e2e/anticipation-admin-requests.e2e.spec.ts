import { test, expect } from '@playwright/test';

// RF-2 Lista global de solicitações de antecipação (Admin) — E2E
// Plano árvore testes RF-2 (T10): CA-RF2-1, CA-RF2-2, CA-RF2-4.
// Cenário 1: Admin acessa lista, vê dados e aplica filtros.
// Cenário 2: Admin abre detalhe a partir da lista.
// Cenário 3: Creator acede à URL da lista e recebe negação de acesso.

test.describe('RF-2 Lista global Admin — E2E', () => {
  const adminListUrl = '/anticipation/list';

  test('CA-RF2-1 / CA-RF2-3: Admin accesses list, sees content and applies filters', async ({
    page,
  }) => {
    const adminEmail = process.env.ADMIN_E2E_EMAIL ?? 'admin@example.com';
    const adminPassword = process.env.ADMIN_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminListUrl)}`);

    await page.getByLabel(/Email corporativo/i).fill(adminEmail);
    await page.getByLabel(/Senha/i).fill(adminPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${adminListUrl.replace(/\//g, '\\/')}`));

    const pageHeading = page.getByRole('heading', { name: /visão Admin/i });
    const emptyMessage = page.getByTestId('admin-page-empty');
    const tableRow = page.locator('[data-testid="admin-requests-table-row"]').first();

    await expect(pageHeading).toBeVisible({ timeout: 10000 });
    await expect(emptyMessage.or(tableRow)).toBeVisible({ timeout: 10000 });

    // Apply filters (creator and/or status)
    await page.getByTestId('admin-filter-status').selectOption({ label: /Em analise/i });
    await page.getByRole('button', { name: /Aplicar filtros/i }).click();

    await expect(
      page.locator('[data-testid="admin-requests-table-row"]').first().or(page.getByTestId('admin-page-empty')),
    ).toBeVisible({ timeout: 10000 });
  });

  test('CA-RF2-4: Admin opens detail from list', async ({ page }) => {
    const adminEmail = process.env.ADMIN_E2E_EMAIL ?? 'admin@example.com';
    const adminPassword = process.env.ADMIN_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminListUrl)}`);
    await page.getByLabel(/Email corporativo/i).fill(adminEmail);
    await page.getByLabel(/Senha/i).fill(adminPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${adminListUrl.replace(/\//g, '\\/')}`));

    const firstRow = page.locator('[data-testid="admin-requests-table-row"]').first();
    await firstRow.click({ timeout: 10000 });

    const detailPanel = page.getByTestId('request-detail');
    await expect(detailPanel).toBeVisible({ timeout: 5000 });
    await expect(detailPanel.getByTestId('request-amount').or(detailPanel.getByTestId('request-detail-creator'))).toBeVisible();
  });

  test('CA-RF2-2: Creator accessing list URL receives access denial', async ({ page }) => {
    const creatorEmail = process.env.CREATOR_E2E_EMAIL ?? 'creator@example.com';
    const creatorPassword = process.env.CREATOR_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminListUrl)}`);
    await page.getByLabel(/Email corporativo/i).fill(creatorEmail);
    await page.getByLabel(/Senha/i).fill(creatorPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    // Creator should be redirected away from /anticipation/list (e.g. to / or /home)
    await expect(page).not.toHaveURL(new RegExp(`.*\\/anticipation\\/list\\/?.*`));

    const adminPage = page.getByTestId('anticipation-admin-requests-page');
    await expect(adminPage).not.toBeVisible();
  });
});

// RF-4 Aprovar/Recusar — Admin aprova e recusa; estado atualizado na UI (CA-RF4-4, CA-RF4-6, CA-RF4-7).
test.describe('RF-4 Aprovar/Recusar — E2E', () => {
  const adminListUrl = '/anticipation/list';

  test('Admin approves Pending request and sees status updated and buttons disappear (CA-RF4-4, CA-RF4-6)', async ({
    page,
  }) => {
    const adminEmail = process.env.ADMIN_E2E_EMAIL ?? 'admin@example.com';
    const adminPassword = process.env.ADMIN_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminListUrl)}`);
    await page.getByLabel(/Email corporativo/i).fill(adminEmail);
    await page.getByLabel(/Senha/i).fill(adminPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${adminListUrl.replace(/\//g, '\\/')}`));

    const firstRow = page.locator('[data-testid="admin-requests-table-row"]').first();
    await firstRow.click({ timeout: 10000 });

    const detailPanel = page.getByTestId('request-detail');
    await expect(detailPanel).toBeVisible({ timeout: 5000 });

    const aprovarBtn = page.getByRole('button', { name: /Aprovar/i });
    await aprovarBtn.click({ timeout: 5000 });

    const confirmBtn = page.getByRole('button', { name: /Confirmar/i });
    if (await confirmBtn.isVisible().catch(() => false)) {
      await confirmBtn.click();
    }

    await expect(detailPanel.getByTestId('request-status')).toContainText(/Aprovada/i, { timeout: 10000 });
    await expect(aprovarBtn).not.toBeVisible();
    await expect(page.getByRole('button', { name: /Recusar/i })).not.toBeVisible();
  });

  test('Admin rejects with reason and sees status Recusada and success message (CA-RF4-4, CA-RF4-7)', async ({
    page,
  }) => {
    const adminEmail = process.env.ADMIN_E2E_EMAIL ?? 'admin@example.com';
    const adminPassword = process.env.ADMIN_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminListUrl)}`);
    await page.getByLabel(/Email corporativo/i).fill(adminEmail);
    await page.getByLabel(/Senha/i).fill(adminPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${adminListUrl.replace(/\//g, '\\/')}`));

    const firstRow = page.locator('[data-testid="admin-requests-table-row"]').first();
    await firstRow.click({ timeout: 10000 });

    const detailPanel = page.getByTestId('request-detail');
    await expect(detailPanel).toBeVisible({ timeout: 5000 });

    const recusarBtn = page.getByRole('button', { name: /Recusar/i });
    await recusarBtn.click({ timeout: 5000 });

    const reasonInput = page.getByTestId('reject-reason').or(page.getByLabel(/motivo/i));
    await reasonInput.fill('Documentação incompleta.');
    await page.getByRole('button', { name: /Confirmar recusa/i }).click();

    await expect(detailPanel.getByTestId('request-status')).toContainText(/Recusada/i, { timeout: 10000 });
    await expect(page.getByText(/solicitação recusada|sucesso|recusada/i)).toBeVisible({ timeout: 5000 });
  });
});
