import * as axe from 'axe-core';

const WCAG_AA_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

export async function expectNoA11yViolations(element: Element): Promise<void> {
  const results = await axe.run(element, {
    runOnly: { type: 'tag', values: WCAG_AA_TAGS }
  });

  const criticalViolations = results.violations.filter(
    (violation) => violation.impact === 'critical'
  );

  expect(criticalViolations).toEqual([]);
}
