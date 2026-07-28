using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;

namespace AngularApi.Contracts.DTO
{
    public class UpdateAppointmentStatusDTO
    {
        public AppointmentStatusEnum? Status { get; set; }
    }
}
