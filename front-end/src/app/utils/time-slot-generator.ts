export interface TimeSlot {
  startTime: Date;
  endTime: Date;
  label: string;
}

export function generateTimeSlots(
  startTime: Date,
  endTime: Date,
  slotDurationMinutes: number
): TimeSlot[] {
  if (slotDurationMinutes <= 0) {
    return [];
  }

  const slots: TimeSlot[] = [];
  const cursor = new Date(startTime);
  const end = new Date(endTime);

  while (cursor < end) {
    const slotEnd = new Date(cursor.getTime() + slotDurationMinutes * 60_000);
    if (slotEnd > end) {
      break;
    }

    slots.push({
      startTime: new Date(cursor),
      endTime: slotEnd,
      label: formatSlotLabel(cursor, slotEnd),
    });

    cursor.setTime(cursor.getTime() + slotDurationMinutes * 60_000);
  }

  return slots;
}

function formatSlotLabel(start: Date, end: Date): string {
  return `${start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - ${end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

export function dayOfWeekName(date: Date): string {
  return date.toLocaleDateString('en-US', { weekday: 'long' });
}
