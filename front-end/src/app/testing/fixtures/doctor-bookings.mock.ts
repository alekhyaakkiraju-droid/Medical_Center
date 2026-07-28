import { mockBookingDTO, mockPagedResult } from '../../../testing/mock-data';
import type { Booking, PagedResult } from '../../pages/models';

const today = new Date();
today.setHours(10, 0, 0, 0);

export const emptyTodaysBookings: PagedResult<Booking> = mockPagedResult([], { totalCount: 0 });
export const singleTodaysBooking: PagedResult<Booking> = mockPagedResult([
  mockBookingDTO({ appointmentId: 101, patientName: 'Alice Patient', appointmentTakenDate: today.toISOString(), appointmentStatus: 'Active' }),
], { totalCount: 1 });
export const multipleTodaysBookings: PagedResult<Booking> = mockPagedResult([
  mockBookingDTO({ appointmentId: 101, patientName: 'Alice Patient', appointmentTakenDate: today.toISOString(), appointmentStatus: 'Active' }),
  mockBookingDTO({ appointmentId: 102, patientName: 'Bob Patient', appointmentTakenDate: new Date(today.getTime() + 3600000).toISOString(), appointmentStatus: 'Pending' }),
  mockBookingDTO({ appointmentId: 103, patientName: 'Carol Patient', appointmentTakenDate: new Date(today.getTime() + 7200000).toISOString(), appointmentStatus: 'Active' }),
], { totalCount: 3 });
export const upcomingBookingsFixture: PagedResult<Booking> = mockPagedResult([
  mockBookingDTO({ appointmentId: 201, patientName: 'Future Patient', appointmentTakenDate: new Date(today.getTime() + 86400000).toISOString(), appointmentStatus: 'Active' }),
  mockBookingDTO({ appointmentId: 202, patientName: 'Later Patient', appointmentTakenDate: new Date(today.getTime() + 172800000).toISOString(), appointmentStatus: 'Pending' }),
], { totalCount: 2 });
