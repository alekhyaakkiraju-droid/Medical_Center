import { NavigationEnd } from '@angular/router';

export const homeNavigationEnd = new NavigationEnd(
  1,
  '/',
  '/'
);

export const aboutNavigationEnd = new NavigationEnd(
  2,
  '/pages/about-us',
  '/pages/about-us'
);

export function navigationEndWithTitle(url: string, id = 1): NavigationEnd {
  return new NavigationEnd(id, url, url);
}
