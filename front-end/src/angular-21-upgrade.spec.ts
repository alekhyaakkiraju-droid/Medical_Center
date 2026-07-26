import { VERSION } from '@angular/core';

describe('Angular 21 upgrade (WO-053)', () => {
  it('reports Angular 21 or newer at runtime', () => {
    expect(Number(VERSION.major)).toBeGreaterThanOrEqual(21);
  });
});
