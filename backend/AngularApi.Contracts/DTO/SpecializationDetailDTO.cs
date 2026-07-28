namespace AngularApi.Contracts.DTO
{
    public class SpecializationDetailDTO
    {
        public int Id { get; set; }
        public string? SpecializationName { get; set; }
        public string? SpecializationImage { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public List<SpecializationServiceItemDTO> Services { get; set; } = new();
    }

    public class SpecializationServiceItemDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
