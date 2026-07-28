import { getRoleBasedRedirectUrl } from './role-redirect.util';

describe('getRoleBasedRedirectUrl', () => {
  it('redirects admin role to admin dashboard', () => {
    expect(getRoleBasedRedirectUrl(['admin'])).toBe('/admin/dashboard');
  });

  it('redirects doctor role to doctor dashboard', () => {
    expect(getRoleBasedRedirectUrl(['doctor'])).toBe('/doctor/dashboard');
  });

  it('redirects patient user role to patient home', () => {
    expect(getRoleBasedRedirectUrl(['user'])).toBe('/patient/home');
  });

  it('prioritizes admin over doctor and user', () => {
    expect(getRoleBasedRedirectUrl(['user', 'doctor', 'admin'])).toBe('/admin/dashboard');
  });

  it('falls back to public home for unknown roles', () => {
    expect(getRoleBasedRedirectUrl([])).toBe('/pages/home');
  });
});
