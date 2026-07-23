namespace AngularApi.DTO
{
    public class SpecializationListItemDTO
    {
        public int Id { get; set; }
        public string? SpecializationName { get; set; }
        public string? SpecializationImage { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
