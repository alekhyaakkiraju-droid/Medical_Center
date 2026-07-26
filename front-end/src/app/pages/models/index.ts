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
export type Review = components['schemas']['ReviewDTO'];
export type SpecializationListItem = components['schemas']['SpecializationListItemDTO'];

/** Not auto-generated — BookingDTO is absent from the OpenAPI spec. */
export interface Booking {
  appointmentId?: number;
  patientId?: string | null;
  patientName?: string | null;
  doctorName?: string | null;
  appointmentTakenDate?: string | null;
  appointmentStatus?: string | null;
}

type GeneratedPagedResultFields = Omit<
  components['schemas']['AppointmentDTOPagedResult'],
  'items'
>;

export type PagedResult<T> = Required<GeneratedPagedResultFields> & {
  items: T[];
};

/** Not auto-generated — anonymous object from /api/Account/user-details. */
export interface UserDetailsResponse {
  email?: string | null;
  userName?: string | null;
  address?: string | null;
  phoneNumber?: string | null;
}

/** Not auto-generated — anonymous object from /api/Appointments/total-earnings. */
export interface TotalEarningsResponse {
  totalEarnings?: number;
}

/** Not auto-generated — message payloads from auth endpoints without response schemas. */
export interface AuthMessageResponse {
  status?: string | null;
  message?: string | null;
}
