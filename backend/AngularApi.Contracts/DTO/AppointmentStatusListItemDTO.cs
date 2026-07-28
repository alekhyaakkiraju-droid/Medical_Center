using AngularApi.Contracts.Enums;
namespace AngularApi.Contracts.DTO
{
    public class AppointmentStatusListItemDTO
    {
        public int Id { get; set; }
        public AppointmentStatusEnum? Status { get; set; }
    }
}
