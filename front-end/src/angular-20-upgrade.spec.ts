import { VERSION } from '@angular/core';

describe('Angular 20 upgrade (WO-052)', () => {
  it('reports Angular 20 or newer at runtime', () => {
    expect(Number(VERSION.major)).toBeGreaterThanOrEqual(20);
  });
});
