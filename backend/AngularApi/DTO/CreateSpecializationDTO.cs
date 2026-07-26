namespace AngularApi.DTO
{
    public class CreateSpecializationDTO
    {
        public string? SpecializationName { get; set; }
        public string? SpecializationImage { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
