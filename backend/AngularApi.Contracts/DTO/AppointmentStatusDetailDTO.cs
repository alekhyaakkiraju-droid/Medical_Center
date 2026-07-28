using AngularApi.Contracts.Enums;
namespace AngularApi.Contracts.DTO
{
    public class AppointmentStatusDetailDTO
    {
        public int Id { get; set; }
        public AppointmentStatusEnum? Status { get; set; }
    }
}
