namespace AngularApi.Options
{
    public class AppointmentSettings
    {
        public const string SectionName = "AppointmentSettings";

        public decimal DefaultFee { get; set; } = 30;
        public int DefaultCenterId { get; set; } = 2;
    }
}
