import { TestBed } from '@angular/core/testing';
import { readFileSync } from 'fs';
import { join } from 'path';

describe('index.html CDN cleanup', () => {
  it('should not reference jQuery, Bootstrap, or Flowbite CDN assets', () => {
    const indexHtml = readFileSync(
      join(process.cwd(), 'src/index.html'),
      'utf-8'
    );

    expect(indexHtml.toLowerCase()).not.toContain('jquery');
    expect(indexHtml.toLowerCase()).not.toContain('bootstrap');
    expect(indexHtml.toLowerCase()).not.toContain('flowbite');
    expect(indexHtml).not.toContain('<script');
  });
});
