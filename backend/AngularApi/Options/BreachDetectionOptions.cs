namespace AngularApi.Options
{
    public class BreachDetectionOptions
    {
        public const string SectionName = "BreachDetection";

        public int MutationThreshold { get; set; } = 50;
        public int WindowMinutes { get; set; } = 5;
        public int FailedAuthThreshold { get; set; } = 15;
    }
}
