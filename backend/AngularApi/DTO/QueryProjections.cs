using AngularApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.DTO
{
    public static class QueryProjections
    {
        public static IQueryable<DoctorDTO> SelectDoctorDto(this IQueryable<Doctor> query) =>
            query.Select(d => new DoctorDTO
            {
                Id = d.Id,
                Name = d.Name,
                Image = d.Image,
                ProfessionalStatement = d.ProfessionalStatement,
                PracticingFrom = d.PracticingFrom,
                Specializations = d.DoctorSpecializations!
                    .Select(ds => ds.Specialization!.SpecializationName!)
                    .ToList()
            });

        public static IQueryable<BookingDTO> SelectBookingDto(this IQueryable<Appointment> query) =>
            query.Select(a => new BookingDTO
            {
                AppointmentId = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient != null ? a.Patient.UserName : a.Name,
                DoctorName = a.DoctorName,
                AppointmentTakenDate = a.AppointmentTakenDate,
                AppointmentStatus = a.AppointmentStatus != null ? a.AppointmentStatus.Status.ToString() : null
            });

        public static IQueryable<AppointmentDTO> SelectAppointmentDto(this IQueryable<Appointment> query) =>
            query.Select(a => new AppointmentDTO
            {
                AppointmentId = a.Id,
                AppointmentDate = a.AppointmentTakenDate ?? a.ProbableStartTime,
                AppointmentTime = a.ProbableStartTime.HasValue
                    ? a.ProbableStartTime.Value.ToString("HH:mm")
                    : null,
                AppointmentStatus = a.AppointmentStatus != null ? a.AppointmentStatus.Status.ToString() : null,
                Doctor = new DoctorDTO
                {
                    Id = a.DoctorId,
                    Name = a.DoctorName,
                    Specializations = a.Doctor != null && a.Doctor.DoctorSpecializations != null
                        ? a.Doctor.DoctorSpecializations
                            .Select(ds => ds.Specialization!.SpecializationName!)
                            .ToList()
                        : new List<string>()
                },
                Patient = new PatientDTO
                {
                    PatientId = a.PatientId,
                    Name = a.Patient != null ? a.Patient.UserName : a.Name,
                    Email = a.Patient != null ? a.Patient.Email : a.Email
                }
            });
    }
}
