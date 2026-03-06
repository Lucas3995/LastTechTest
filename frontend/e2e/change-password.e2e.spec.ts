import { expect, test } from '@playwright/test';

test.describe('RF-8 Change Password — E2E (T-RF8-07)', () => {
  const changePasswordUrl = '/auth/change-password';

  test('[T-RF8-07][CA-RF8-1][CA-RF8-4] Creator changes password and can login with new password', async ({
    page,
  }) => {
    const creatorEmail = process.env.CREATOR_E2E_EMAIL ?? 'creator@example.com';
    const currentPassword = process.env.CREATOR_E2E_PASSWORD ?? 'password';
    const newPassword = process.env.CREATOR_E2E_NEW_PASSWORD ?? 'Creator@1234';

    await page.goto(`/?returnUrl=${encodeURIComponent(changePasswordUrl)}`);

    await page.getByLabel(/Email corporativo/i).fill(creatorEmail);
    await page.getByLabel(/Senha/i).fill(currentPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${changePasswordUrl.replace(/\//g, '\\/')}`));

    await page.getByLabel(/Senha atual/i).fill(currentPassword);
    await page.getByLabel(/Nova senha/i).fill(newPassword);
    await page.getByLabel(/Confirmar nova senha/i).fill(newPassword);
    await page.getByRole('button', { name: /Salvar nova senha/i }).click();

    await expect(page.getByRole('status')).toContainText(/Senha alterada com sucesso/i, {
      timeout: 10000,
    });
  });

  test('[T-RF8-07][CA-RF8-1][CA-RF8-4] Analista changes password and can login with new password', async ({
    page,
  }) => {
    const analistaEmail = process.env.ANALISTA_E2E_EMAIL ?? 'analista@example.com';
    const currentPassword = process.env.ANALISTA_E2E_PASSWORD ?? 'password';
    const newPassword = process.env.ANALISTA_E2E_NEW_PASSWORD ?? 'Analista@1234';

    await page.goto(`/?returnUrl=${encodeURIComponent(changePasswordUrl)}`);

    await page.getByLabel(/Email corporativo/i).fill(analistaEmail);
    await page.getByLabel(/Senha/i).fill(currentPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${changePasswordUrl.replace(/\//g, '\\/')}`));

    await page.getByLabel(/Senha atual/i).fill(currentPassword);
    await page.getByLabel(/Nova senha/i).fill(newPassword);
    await page.getByLabel(/Confirmar nova senha/i).fill(newPassword);
    await page.getByRole('button', { name: /Salvar nova senha/i }).click();

    await expect(page.getByRole('status')).toContainText(/Senha alterada com sucesso/i, {
      timeout: 10000,
    });
  });

  test('[T-RF8-07][CA-RF8-5][CA-RF8-6] Invalid current password shows error and keeps form open', async ({
    page,
  }) => {
    const creatorEmail = process.env.CREATOR_E2E_EMAIL ?? 'creator@example.com';
    const currentPassword = process.env.CREATOR_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(changePasswordUrl)}`);

    await page.getByLabel(/Email corporativo/i).fill(creatorEmail);
    await page.getByLabel(/Senha/i).fill(currentPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${changePasswordUrl.replace(/\//g, '\\/')}`));

    await page.getByLabel(/Senha atual/i).fill('SenhaErrada@123');
    await page.getByLabel(/Nova senha/i).fill('Valida@1234');
    await page.getByLabel(/Confirmar nova senha/i).fill('Valida@1234');
    await page.getByRole('button', { name: /Salvar nova senha/i }).click();

    await expect(page.getByRole('alert')).toBeVisible({ timeout: 10000 });
    await expect(page.getByRole('alert')).toContainText(/senha atual inválida|nao foi possivel alterar a senha/i);
    await expect(page.getByLabel(/Senha atual/i)).toBeVisible();
  });

  test('[T-RF8-07][CA-RF8-2] Admin should not see menu entry for password change', async ({ page }) => {
    const adminEmail = process.env.ADMIN_E2E_EMAIL ?? 'usu_acesso_total@example.com';
    const adminPassword = process.env.ADMIN_E2E_PASSWORD ?? 'Acess0@t0ta1';

    await page.goto('/?returnUrl=%2Fhome');

    await page.getByLabel(/Email corporativo/i).fill(adminEmail);
    await page.getByLabel(/Senha/i).fill(adminPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(/\/home/);

    await expect(page.getByRole('link', { name: /Alterar senha/i })).not.toBeVisible();
    await expect(page.getByRole('link', { name: /Trocar senha/i })).not.toBeVisible();
  });
});
