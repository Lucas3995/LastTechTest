describe('AdminFacade — RF-6 (T5)', () => {
  async function loadAdminFacadeType(): Promise<new (...args: unknown[]) => unknown> {
    const target = `./${'admin.facade'}`;
    const module = (await import(/* @vite-ignore */ target)) as {
      AdminFacade?: new (...args: unknown[]) => unknown;
    };
    expect(module.AdminFacade).toBeTruthy();
    return module.AdminFacade as new (...args: unknown[]) => unknown;
  }

  it('RF6 T5: should expose state API (signals/selectors)', async () => {
    const facadeType = await loadAdminFacadeType();
    const prototype = facadeType.prototype as Record<string, unknown>;

    expect(typeof prototype['users']).toBe('function');
    expect(typeof prototype['isLoading']).toBe('function');
    expect(typeof prototype['errorMessage']).toBe('function');
    expect(typeof prototype['searchTerm']).toBe('function');
  });

  it('RF6 T5: should expose debounced search API', async () => {
    const facadeType = await loadAdminFacadeType();
    const prototype = facadeType.prototype as Record<string, unknown>;

    expect(typeof prototype['setSearchTerm']).toBe('function');
    expect(typeof prototype['loadUsers']).toBe('function');
  });

  it('RF6 T5: should expose create/reset workflows with success and error channels', async () => {
    const facadeType = await loadAdminFacadeType();
    const prototype = facadeType.prototype as Record<string, unknown>;

    expect(typeof prototype['createUser']).toBe('function');
    expect(typeof prototype['resetPassword']).toBe('function');
    expect(typeof prototype['infoMessage']).toBe('function');
    expect(typeof prototype['clearMessages']).toBe('function');
  });
});
