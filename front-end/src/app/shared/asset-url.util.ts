import { environment } from '../../environments/environment';

/** Resolves a static asset path against the configured asset base URL. */
export function resolveAssetUrl(path: string | null | undefined): string {
  if (path == null) {
    return '';
  }

  const trimmedPath = path.trim();
  if (!trimmedPath) {
    return '';
  }

  if (/^(https?:|data:|\/\/)/i.test(trimmedPath)) {
    return trimmedPath;
  }

  const base = (environment.assetBaseUrl ?? '').trim();
  if (!base) {
    return trimmedPath.startsWith('/') ? trimmedPath : `/${trimmedPath}`;
  }

  const normalizedBase = base.replace(/\/+$/, '');
  const normalizedPath = trimmedPath.replace(/^\/+/, '');
  return `${normalizedBase}/${normalizedPath}`;
}
