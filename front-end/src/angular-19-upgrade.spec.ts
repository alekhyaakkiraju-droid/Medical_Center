import { VERSION } from '@angular/core';

describe('Angular 19 upgrade (WO-051)', () => {
  it('reports Angular 19 or newer at runtime', () => {
    expect(Number(VERSION.major)).toBeGreaterThanOrEqual(19);
  });
});
