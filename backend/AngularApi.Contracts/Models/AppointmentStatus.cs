using AngularApi.Contracts.Enums;

namespace AngularApi.Contracts.Models;

public class AppointmentStatus
{
    public int Id { get; set; }
    public AppointmentStatusEnum? Status { get; set; }
}
