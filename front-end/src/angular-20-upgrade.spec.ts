import { VERSION } from '@angular/core';

describe('Angular 20 upgrade (WO-052)', () => {
  it('reports Angular 20.x at runtime', () => {
    expect(VERSION.major).toBe('20');
  });
});
