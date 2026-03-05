/**
 * Core layer: singletons, global config, guards.
 * Prefer depending only on domain here.
 */

export * from './auth/auth.service';
export * from './auth/auth.guard';
export * from './auth/auth-redirect.guard';
