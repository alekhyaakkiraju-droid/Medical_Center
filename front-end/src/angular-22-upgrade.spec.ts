import { VERSION } from '@angular/core';

describe('Angular 22 upgrade (WO-054)', () => {
  it('reports Angular 22.x at runtime', () => {
    expect(VERSION.major).toBe('22');
  });
});
