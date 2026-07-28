export function getRoleBasedRedirectUrl(roles: string[]): string {
  if (roles.includes('admin')) {
    return '/admin/dashboard';
  }
  if (roles.includes('doctor')) {
    return '/doctor/dashboard';
  }
  if (roles.includes('user')) {
    return '/patient/home';
  }
  return '/pages/home';
}
