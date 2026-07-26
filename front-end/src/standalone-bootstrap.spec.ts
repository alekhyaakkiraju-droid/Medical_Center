import { readFileSync } from 'node:fs';
import { join } from 'node:path';

describe('Standalone bootstrap (WO-056)', () => {
  it('bootstraps via bootstrapApplication in main.ts', () => {
    const mainSource = readFileSync(join(__dirname, 'main.ts'), 'utf8');
    expect(mainSource).toContain('bootstrapApplication');
    expect(mainSource).toContain("from './app/app.config'");
    expect(mainSource).not.toContain('bootstrapModule');
    expect(mainSource).not.toContain('AppModule');
  });

  it('defines application providers in app.config.ts', () => {
    const configSource = readFileSync(join(__dirname, 'app', 'app.config.ts'), 'utf8');
    expect(configSource).toContain('export const appConfig');
    expect(configSource).toContain('provideRouter(routes)');
    expect(configSource).toContain('provideHttpClient');
  });

  it('uses route files instead of NgModule lazy loading', () => {
    const routesSource = readFileSync(join(__dirname, 'app', 'app.routes.ts'), 'utf8');
    expect(routesSource).toContain('admin.routes');
    expect(routesSource).toContain('doctor.routes');
    expect(routesSource).toContain('general.routes');
    expect(routesSource).toContain('auth.routes');
    expect(routesSource).not.toContain('.module');
  });
});
