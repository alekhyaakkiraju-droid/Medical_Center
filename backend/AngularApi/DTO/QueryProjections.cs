using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
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

        public static IQueryable<DoctorDetailDTO> SelectDoctorDetailDto(this IQueryable<Doctor> query) =>
            query.Select(d => new DoctorDetailDTO
            {
                Id = d.Id,
                Name = d.Name,
                Image = d.Image,
                ProfessionalStatement = d.ProfessionalStatement,
                PracticingFrom = d.PracticingFrom,
                Specializations = d.DoctorSpecializations!
                    .Select(ds => ds.Specialization!.SpecializationName!)
                    .ToList(),
                Qualifications = d.Qualifications!
                    .Select(q => new DoctorQualificationSummary(
                        q.QualificationName,
                        q.InstituteName,
                        q.ProcurementYear))
                    .ToList(),
                HospitalAffiliations = d.HospitalAffiliations!
                    .Select(h => new HospitalAffiliationSummary(
                        h.HospitalName,
                        h.City,
                        h.Country,
                        h.StartDate,
                        h.EndDate))
                    .ToList(),
                AverageRating = d.PatientReviews != null && d.PatientReviews.Any()
                    ? d.PatientReviews.Average(r => (double?)r.OverallRating) ?? 0d
                    : 0d
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

        public static IQueryable<SpecializationDetailDTO> SelectSpecializationDetailDto(this IQueryable<Specialization> query) =>
            query.Select(s => new SpecializationDetailDTO
            {
                Id = s.Id,
                SpecializationName = s.SpecializationName,
                SpecializationImage = s.SpecializationImage,
                Description = s.Description,
                IsActive = s.IsActive,
                Services = s.Services!
                    .Select(svc => new SpecializationServiceItemDTO
                    {
                        Id = svc.Id,
                        Name = svc.Name,
                        Description = svc.Description
                    })
                    .ToList()
            });

        public static IQueryable<AppointmentStatusDetailDTO> SelectAppointmentStatusDetailDto(this IQueryable<AppointmentStatus> query) =>
            query.Select(s => new AppointmentStatusDetailDTO
            {
                Id = s.Id,
                Status = s.Status
            });

        public static IQueryable<MedicalCenterDetailDTO> SelectMedicalCenterDetailDto(this IQueryable<MedicalCenter> query) =>
            query.Select(m => new MedicalCenterDetailDTO
            {
                Id = m.Id,
                HospitalAffiliationId = m.HospitalAffiliationId,
                TimeSlotPerClientInMin = m.TimeSlotPerClientInMin,
                FirstConsultationFee = m.FirstConsultationFee,
                FollowupConsultationFee = m.FollowupConsultationFee,
                StreetAddress = m.StreetAddress,
                City = m.City,
                State = m.State,
                Zip = m.Zip
            });

        public static IQueryable<MedicalCenterDoctorAvailabilityDetailDTO> SelectMedicalCenterDoctorAvailabilityDetailDto(
            this IQueryable<MedicalCenterDoctorAvailability> query) =>
            query.Select(a => new MedicalCenterDoctorAvailabilityDetailDTO
            {
                Id = a.Id,
                MedicalCenterId = a.MedicalCenterId,
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsAvailable = a.IsAvailable,
                ReasonOfUnavailability = a.ReasonOfUnavailability
            });

        public static IQueryable<PatientReviewDetailDTO> SelectPatientReviewDetailDto(this IQueryable<PatientReview> query) =>
            query.Select(r => new PatientReviewDetailDTO
            {
                Id = r.Id,
                PatientId = r.IsReviewAnonymous == true ? null : r.PatientId,
                DoctorId = r.DoctorId,
                IsReviewAnonymous = r.IsReviewAnonymous,
                WaitTimeRating = r.WaitTimeRating,
                BedsideMannerRating = r.BedsideMannerRating,
                OverallRating = r.OverallRating,
                Review = r.Review,
                IsDoctorRecommended = r.IsDoctorRecommended,
                ReviewDate = r.ReviewDate
            });
    }
}
