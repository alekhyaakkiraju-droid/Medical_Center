import type { components } from '../../api/generated/api';

export type Doctor = components['schemas']['DoctorDTO'];
export type Profile = components['schemas']['UpdateProfileDto'];
export type ProfileDetails = Profile & {
  coverImgUrl?: string | null;
  personalImgUrl?: string | null;
};
export type Appointment = components['schemas']['AppointmentDTO'];
export type UpdateAppointment = components['schemas']['UpdateAppointmentDTO'];
export type Patient = components['schemas']['PatientDTO'];
export interface Booking {
  appointmentId?: number;
  patientId?: string | null;
  patientName?: string | null;
  doctorName?: string | null;
  appointmentTakenDate?: string | null;
  appointmentStatus?: string | null;
}
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageCount: number;
  currentPage: number;
  pageSize: number;
}
