import { AdminFacade } from './admin.facade';

describe('AdminFacade — RF-6 (T5)', () => {
  it('RF6 T5: should expose state API (signals/selectors)', () => {
    const prototype = AdminFacade.prototype as unknown as Record<string, unknown>;

    expect(typeof prototype['users']).toBe('function');
    expect(typeof prototype['isLoading']).toBe('function');
    expect(typeof prototype['errorMessage']).toBe('function');
    expect(typeof prototype['searchTerm']).toBe('function');
  });

  it('RF6 T5: should expose debounced search API', () => {
    const prototype = AdminFacade.prototype as unknown as Record<string, unknown>;

    expect(typeof prototype['setSearchTerm']).toBe('function');
    expect(typeof prototype['loadUsers']).toBe('function');
  });

  it('RF6 T5: should expose create/reset workflows with success and error channels', () => {
    const prototype = AdminFacade.prototype as unknown as Record<string, unknown>;

    expect(typeof prototype['createUser']).toBe('function');
    expect(typeof prototype['resetPassword']).toBe('function');
    expect(typeof prototype['infoMessage']).toBe('function');
    expect(typeof prototype['clearMessages']).toBe('function');
  });
});
