import { expect, test } from '@playwright/test';

test.describe('RF-6 Gestão de Usuários (Admin) — E2E', () => {
  const adminUsersUrl = '/admin/users';

  test('RF6 T7 / US-G1..G3: admin should list users, create user and reset password', async ({ page }) => {
    const adminEmail = process.env.ADMIN_E2E_EMAIL ?? 'usu_acesso_total@example.com';
    const adminPassword = process.env.ADMIN_E2E_PASSWORD ?? 'Acess0@t0ta1';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminUsersUrl)}`);

    await page.getByLabel(/Email corporativo/i).fill(adminEmail);
    await page.getByLabel(/Senha/i).fill(adminPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).toHaveURL(new RegExp(`.*${adminUsersUrl.replace(/\//g, '\\/')}`));
    await expect(page.getByRole('heading', { name: /Gestão de Usuários/i })).toBeVisible({ timeout: 10000 });

    await expect(page.getByRole('button', { name: /Novo Usuário/i })).toBeVisible();
    await page.getByRole('button', { name: /Novo Usuário/i }).click();

    const uniqueEmail = `rf6-e2e-${Date.now()}@example.com`;
    await page.getByLabel(/E-mail|Email/i).fill(uniqueEmail);
    await page.getByLabel(/Papel|Role/i).click();
    await page.getByRole('option', { name: /Creator/i }).click();
    await page.getByRole('button', { name: /Salvar|Criar/i }).click();

    await expect(page.getByText(uniqueEmail)).toBeVisible({ timeout: 10000 });

    await page.getByRole('row', { name: new RegExp(uniqueEmail, 'i') }).getByRole('button', { name: /Resetar Senha|Reset/i }).click();
    await page.getByRole('button', { name: /Confirmar|Sim/i }).click();

    await expect(page.getByText(/Trocar@123/i)).toBeVisible({ timeout: 10000 });
  });

  test('RF6 T7 / US-G4: creator should be blocked from /admin/users', async ({ page }) => {
    const creatorEmail = process.env.CREATOR_E2E_EMAIL ?? 'creator@example.com';
    const creatorPassword = process.env.CREATOR_E2E_PASSWORD ?? 'password';

    await page.goto(`/?returnUrl=${encodeURIComponent(adminUsersUrl)}`);
    await page.getByLabel(/Email corporativo/i).fill(creatorEmail);
    await page.getByLabel(/Senha/i).fill(creatorPassword);
    await page.getByRole('button', { name: /Entrar/i }).click();

    await expect(page).not.toHaveURL(new RegExp(`.*\\/admin\\/users\\/?.*`));
    await expect(page.getByRole('heading', { name: /Gestão de Usuários/i })).not.toBeVisible();
  });
});
