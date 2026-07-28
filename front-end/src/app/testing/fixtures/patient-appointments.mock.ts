import { mockAppointmentDTO, mockPagedResult } from '../../../testing/mock-data';
import type { Appointment, PagedResult } from '../../pages/models';

const tomorrow = new Date();
tomorrow.setDate(tomorrow.getDate() + 1);

export const emptyPatientAppointments: PagedResult<Appointment> = mockPagedResult([], { totalCount: 0 });

export const upcomingPatientAppointments: PagedResult<Appointment> = mockPagedResult([
  mockAppointmentDTO({ appointmentId: 1, appointmentStatus: 'Active', appointmentDate: tomorrow.toISOString(), doctor: { id: 'd1', name: 'Dr. Smith', image: null, professionalStatement: null, practicingFrom: null, specializations: [] } }),
  mockAppointmentDTO({ appointmentId: 2, appointmentStatus: 'Pending', appointmentDate: new Date(tomorrow.getTime() + 86400000).toISOString() }),
], { totalCount: 2 });

export const mixedPatientAppointments: PagedResult<Appointment> = mockPagedResult([
  mockAppointmentDTO({ appointmentId: 3, appointmentStatus: 'Completed', appointmentDate: '2026-01-01T10:00:00Z' }),
  mockAppointmentDTO({ appointmentId: 4, appointmentStatus: 'Cancelled', appointmentDate: '2026-02-01T10:00:00Z' }),
  mockAppointmentDTO({ appointmentId: 5, appointmentStatus: 'Active', appointmentDate: tomorrow.toISOString() }),
], { totalCount: 3 });
