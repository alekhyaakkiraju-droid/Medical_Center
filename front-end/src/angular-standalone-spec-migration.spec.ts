import * as fs from 'node:fs';
import * as path from 'node:path';

describe('Standalone spec migration (WO-057)', () => {
  const appRoot = path.join(__dirname, 'app');
  const deletedModulePattern = /(SharedModule|AdminModule|DoctorModule|GeneralModule|AuthModule|AppModule)/;

  const specFiles = collectSpecFiles(appRoot);

  it('includes component and accessibility spec files', () => {
    expect(specFiles.length).toBeGreaterThanOrEqual(28);
    expect(specFiles.some((file) => file.endsWith('accessibility.spec.ts'))).toBeTrue();
  });

  it('does not reference deleted NgModules in spec files', () => {
    const offenders = specFiles.filter((file) => deletedModulePattern.test(fs.readFileSync(file, 'utf8')));
    expect(offenders).withContext(`NgModule references found in: ${offenders.join(', ')}`).toEqual([]);
  });

  it('does not use declarations arrays for standalone components', () => {
    const offenders = specFiles.filter((file) => /declarations:\s*\[/.test(fs.readFileSync(file, 'utf8')));
    expect(offenders).withContext(`declarations: found in: ${offenders.join(', ')}`).toEqual([]);
  });

  it('marks accessibility stub components as standalone', () => {
    const accessibilitySpec = fs.readFileSync(path.join(appRoot, 'accessibility', 'accessibility.spec.ts'), 'utf8');
    const stubCount = (accessibilitySpec.match(/standalone:\s*true/g) ?? []).length;
    expect(stubCount).toBe(4);
  });
});

function collectSpecFiles(directory: string): string[] {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      return collectSpecFiles(fullPath);
    }

    return entry.name.endsWith('.spec.ts') ? [fullPath] : [];
  });
}
