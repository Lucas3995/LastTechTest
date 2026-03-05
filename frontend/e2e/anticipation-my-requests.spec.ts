import { test, expect } from '@playwright/test';

// RF-1 Minhas solicitações de antecipação — E2E
// Estes testes foram escritos antes da tela existir, como especificação executável.
// A rota e os seletores de UI deverão ser ajustados quando a implementação Angular estiver disponível.
// Plano 401: teste "creator_logs_in_then_navigates_to_my_requests_and_page_loads_without_401" cobre
// critério "após login, Minhas solicitações carrega com 200 (não 401)".

test.describe('RF-1 Minhas solicitacoes de antecipacao (Creator) — E2E', () => {
  const myRequestsUrl = '/anticipation/my-requests';

  test('Plano 401: creator logs in then navigates to my-requests and page loads without 401', async ({
    page,
  }) => {
    const email = process.env.CREATOR_E2E_EMAIL ?? 'creator@example.com';
    const password = process.env.CREATOR_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(myRequestsUrl)}`);

    await page.getByLabel(/Email corporativo/i).fill(email);
    await page.getByLabel(/Senha/i).fill(password);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${myRequestsUrl.replace(/\//g, '\\/')}`));

    const emptyState = page.getByTestId('requests-empty-state');
    const tableRow = page.locator('[data-testid="requests-table-row"]').first();
    const heading = page.getByRole('heading', { name: /Minhas solicita/i });

    await expect(emptyState.or(tableRow).or(heading)).toBeVisible({ timeout: 10000 });
  });

  test('CA-RF1-1: creator sees only own requests in list', async ({ page }) => {
    await page.goto(myRequestsUrl);

    // TODO: ajustar seletores quando a tela existir
    const rows = page.locator('[data-testid="requests-table-row"]');
    await expect(rows).toHaveCountGreaterThan(0);

    // Exemplo de verificação futura de pertencimento ao creator (por texto ou atributo)
    // await expect(rows.filter({ hasText: 'another-creator' })).toHaveCount(0);
  });

  test('CA-RF1-2: creator without requests sees empty state and CTAs', async ({ page }) => {
    await page.goto(myRequestsUrl + '?fixture=empty');

    const emptyState = page.getByTestId('requests-empty-state');
    await expect(emptyState).toBeVisible();
    await expect(emptyState.getByRole('button', { name: /Nova solicitacao/i })).toBeVisible();
    await expect(
      emptyState.getByRole('button', { name: /Simular antecipacao/i })
    ).toBeVisible();
  });

  test('CA-RF1-3: creator applies filters and sorting and sees consistent UI', async ({ page }) => {
    await page.goto(myRequestsUrl + '?fixture=many');

    // Filtros por status
    await page.getByTestId('filter-status').click();
    await page.getByRole('option', { name: /Em analise/i }).click();
    await page.getByRole('button', { name: /Aplicar filtros/i }).click();

    const filteredRows = page.locator('[data-testid="requests-table-row"]');
    await expect(filteredRows).toHaveCountGreaterThan(0);

    // Ordenação por valor
    await page.getByRole('columnheader', { name: /Valor/i }).click();
    await expect(
      page.getByRole('columnheader', { name: /Valor/i })
    ).toHaveAttribute('aria-sort', /ascending|descending/);
  });

  test('CA-RF1-4: creator opens request detail and sees expected information', async ({ page }) => {
    await page.goto(myRequestsUrl + '?fixture=default');

    const firstRow = page.locator('[data-testid="requests-table-row"]').first();
    await firstRow.click();

    const detailPanel = page.getByTestId('request-detail');
    await expect(detailPanel).toBeVisible();
    await expect(detailPanel.getByTestId('request-amount')).toBeVisible();
    await expect(detailPanel.getByTestId('request-status')).toBeVisible();
  });

  test('CA-RF1-4: creator sees friendly error when detail service fails', async ({ page }) => {
    await page.goto(myRequestsUrl + '?fixture=detail-error');

    const firstRow = page.locator('[data-testid="requests-table-row"]').first();
    await firstRow.click();

    const errorBanner = page.getByRole('alert');
    await expect(errorBanner).toBeVisible();
    await expect(errorBanner).toContainText(/Nao foi possivel carregar os detalhes/i);
  });

  test('CA-RF1-5: creator cancels pending request and sees updated status', async ({ page }) => {
    await page.goto(myRequestsUrl + '?fixture=pending');

    const firstRow = page.locator('[data-testid="requests-table-row"]').first();
    await firstRow.click();

    const detailPanel = page.getByTestId('request-detail');
    const cancelButton = detailPanel.getByRole('button', { name: /Cancelar solicitacao/i });
    await expect(cancelButton).toBeEnabled();

    await cancelButton.click();

    const confirmDialog = page.getByRole('dialog');
    await expect(confirmDialog).toBeVisible();
    await expect(confirmDialog).toContainText(/Esta acao nao podera ser desfeita/i);

    await confirmDialog.getByRole('button', { name: /Confirmar/i }).click();

    await expect(detailPanel.getByTestId('request-status')).toContainText(
      /Cancelada pelo creator/i
    );
    await expect(firstRow.getByTestId('request-status-tag')).toContainText(
      /Cancelada pelo creator/i
    );
  });

  test('CA-RF1-6: creator attempts to cancel again and sees already canceled message', async ({
    page,
  }) => {
    await page.goto(myRequestsUrl + '?fixture=canceled');

    const firstRow = page.locator('[data-testid="requests-table-row"]').first();
    await firstRow.click();

    const detailPanel = page.getByTestId('request-detail');
    const cancelButton = detailPanel.getByRole('button', { name: /Cancelar solicitacao/i });

    // Dependendo da UX final, o botão pode estar desabilitado ou oculto;
    // aqui assumimos que ainda aparece, mas com comportamento idempotente.
    await expect(cancelButton).toBeVisible();
    await cancelButton.click();

    const infoBanner = page.getByRole('status');
    await expect(infoBanner).toBeVisible();
    await expect(infoBanner).toContainText(/Esta solicitacao ja foi cancelada/i);
  });
});

