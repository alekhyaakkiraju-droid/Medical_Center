import type { components } from '../app/api/generated/api';
import type { Appointment, Booking, Doctor, PagedResult, Patient, ProfileDetails } from '../app/pages/models';

export type ReviewDTO = components['schemas']['ReviewDTO'];

export function mockDoctorDTO(overrides: Partial<Doctor> = {}): Doctor {
  return {
    id: 'doctor-1',
    name: 'Dr. Contract Test',
    image: null,
    professionalStatement: 'Board certified physician',
    practicingFrom: '2010-01-01T00:00:00Z',
    specializations: ['Cardiology'],
    ...overrides,
  };
}

export function mockAppointmentDTO(overrides: Partial<Appointment> = {}): Appointment {
  return {
    appointmentId: 1,
    appointmentDate: '2026-08-01T00:00:00Z',
    appointmentTime: '10:00',
    appointmentStatus: 'Pending',
    doctor: mockDoctorDTO(),
    patient: mockPatientDTO(),
    ...overrides,
  };
}

export function mockPatientDTO(overrides: Partial<Patient> = {}): Patient {
  return {
    patientId: 'patient-1',
    name: 'Test Patient',
    email: 'patient@example.com',
    image: null,
    reviews: [],
    ...overrides,
  };
}

export function mockReviewDTO(overrides: Partial<ReviewDTO> = {}): ReviewDTO {
  return {
    id: 1,
    patientId: 'patient-1',
    doctorId: 'doctor-1',
    isReviewAnonymous: false,
    waitTimeRating: 5,
    bedsideMannerRating: 5,
    overallRating: 5,
    review: 'Excellent care',
    isDoctorRecommended: true,
    reviewDate: '2026-07-26T00:00:00Z',
    ...overrides,
  };
}

export function mockBookingDTO(overrides: Partial<Booking> = {}): Booking {
  return {
    appointmentId: 1,
    patientId: 'patient-1',
    patientName: 'Test Patient',
    doctorName: 'Dr. Contract Test',
    appointmentTakenDate: '2026-08-01T00:00:00Z',
    appointmentStatus: 'Pending',
    ...overrides,
  };
}

export function mockPagedResult<T>(items: T[], overrides: Partial<PagedResult<T>> = {}): PagedResult<T> {
  return {
    items,
    totalCount: items.length,
    pageCount: 1,
    currentPage: 1,
    pageSize: 10,
    ...overrides,
  };
}

export function mockProfileDetails(overrides: Partial<ProfileDetails> = {}): ProfileDetails {
  return {
    userName: 'Test User',
    email: 'user@example.com',
    phoneNumber: '5551234567',
    address: '123 Main St',
    coverImgUrl: '',
    personalImgUrl: '',
    ...overrides,
  };
}
