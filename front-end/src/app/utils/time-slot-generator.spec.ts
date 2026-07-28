import { generateTimeSlots, dayOfWeekName } from './time-slot-generator';

describe('time-slot-generator', () => {
  it('generates 30-minute slots between 9am and 11am', () => {
    const start = new Date('2026-08-15T09:00:00');
    const end = new Date('2026-08-15T11:00:00');

    const slots = generateTimeSlots(start, end, 30);

    expect(slots).toHaveLength(4);
    expect(slots[0].startTime.getHours()).toBe(9);
    expect(slots[0].endTime.getMinutes()).toBe(30);
  });

  it('returns empty array when duration is zero', () => {
    const start = new Date('2026-08-15T09:00:00');
    const end = new Date('2026-08-15T11:00:00');

    expect(generateTimeSlots(start, end, 0)).toEqual([]);
  });

  it('handles uneven division by excluding partial trailing slot', () => {
    const start = new Date('2026-08-15T09:00:00');
    const end = new Date('2026-08-15T10:20:00');

    const slots = generateTimeSlots(start, end, 30);

    expect(slots).toHaveLength(2);
  });

  it('derives day of week name from selected date', () => {
    expect(dayOfWeekName(new Date('2026-08-15T12:00:00'))).toBe('Saturday');
  });
});
