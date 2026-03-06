import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { vi } from 'vitest';

import { API_BASE_URL } from '../../core/api-base-url';
import { AdminService } from './admin.service';

describe('AdminService — RF-6 (T4)', () => {
  const BASE_URL = 'http://localhost:5114';

  it('RF6 T4: should expose listUsers/createUser/resetPassword contract', () => {
    const prototype = AdminService.prototype as unknown as Record<string, unknown>;

    expect(typeof prototype['listUsers']).toBe('function');
    expect(typeof prototype['createUser']).toBe('function');
    expect(typeof prototype['resetPassword']).toBe('function');
  });

  it('RF6 T4: listUsers should call GET /auth/admin/users', () => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AdminService, { provide: API_BASE_URL, useValue: BASE_URL }],
    });

    const service = TestBed.inject(AdminService) as { listUsers(): unknown };
    const httpMock = TestBed.inject(HttpTestingController);

    (service.listUsers() as { subscribe(cb?: () => void): void }).subscribe();

    const req = httpMock.expectOne(`${BASE_URL}/auth/admin/users`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
    httpMock.verify();
  });

  it('RF6 T4: createUser should call POST /auth/admin/users and bubble 400', () => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AdminService, { provide: API_BASE_URL, useValue: BASE_URL }],
    });

    const service = TestBed.inject(AdminService) as {
      createUser(payload: { email: string; roles: string[] }): { subscribe(observer: { next: () => void; error: (error: unknown) => void }): void };
    };
    const httpMock = TestBed.inject(HttpTestingController);

    const nextSpy = vi.fn();
    const errorSpy = vi.fn();

    service.createUser({ email: 'duplicated@example.com', roles: ['Creator'] }).subscribe({
      next: nextSpy,
      error: errorSpy,
    });

    const req = httpMock.expectOne(`${BASE_URL}/auth/admin/users`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      email: 'duplicated@example.com',
      roles: ['Creator'],
    });
    req.flush({ error: 'User already exists' }, { status: 400, statusText: 'Bad Request' });

    expect(nextSpy).not.toHaveBeenCalled();
    expect(errorSpy).toHaveBeenCalled();
    httpMock.verify();
  });

  it('RF6 T4: resetPassword should call POST /auth/admin/users/reset-password', () => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AdminService, { provide: API_BASE_URL, useValue: BASE_URL }],
    });

    const service = TestBed.inject(AdminService) as {
      resetPassword(payload: { email: string }): { subscribe(cb?: () => void): void };
    };
    const httpMock = TestBed.inject(HttpTestingController);

    service.resetPassword({ email: 'user-to-reset@example.com' }).subscribe();

    const req = httpMock.expectOne(`${BASE_URL}/auth/admin/users/reset-password`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'user-to-reset@example.com' });
    req.flush({});
    httpMock.verify();
  });
});
