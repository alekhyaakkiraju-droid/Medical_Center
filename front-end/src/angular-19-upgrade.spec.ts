import { VERSION } from '@angular/core';

describe('Angular 19 upgrade (WO-051)', () => {
  it('reports Angular 19.x at runtime', () => {
    expect(VERSION.major).toBe('19');
  });
});
