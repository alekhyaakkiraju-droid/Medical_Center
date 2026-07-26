import { TestBed } from '@angular/core/testing';
import { CsrfTokenStore } from './csrf-token.store';
import { standaloneComponentTestProviders } from '../testing/standalone-component-test-providers';

describe('CsrfTokenStore', () => {
  let store: CsrfTokenStore;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    store = TestBed.inject(CsrfTokenStore);
  });

  it('should store and return a CSRF token', () => {
    store.setToken('csrf-token');
    expect(store.getToken()).toBe('csrf-token');
  });

  it('should clear the CSRF token', () => {
    store.setToken('csrf-token');
    store.clearToken();
    expect(store.getToken()).toBeNull();
  });
});
