using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;

namespace AngularApi.Contracts.DTO
{
    public class CreateAppointmentStatusDTO
    {
        public AppointmentStatusEnum? Status { get; set; }
    }
}
