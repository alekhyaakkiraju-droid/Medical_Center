namespace AngularApi.Contracts.DTO;

public class CreateAppointmentDTO
{
    public string DoctorId { get; set; } = string.Empty;
    public int MedicalCenterId { get; set; }
    public DateTime AppointmentTakenDate { get; set; }
    public DateTime ProbableStartTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
