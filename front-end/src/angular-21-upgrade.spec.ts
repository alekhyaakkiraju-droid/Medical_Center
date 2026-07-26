import { VERSION } from '@angular/core';

describe('Angular 21 upgrade (WO-053)', () => {
  it('reports Angular 21.x at runtime', () => {
    expect(VERSION.major).toBe('21');
  });
});
