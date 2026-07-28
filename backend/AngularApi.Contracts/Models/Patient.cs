using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace AngularApi.Contracts.Models
{
    public class Patient : AppUser, IAuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? Address { get; set; }
        [JsonIgnore]
        public ICollection<PatientReview>? PatientReview { get; set; } = new List<PatientReview>();
    }

}
