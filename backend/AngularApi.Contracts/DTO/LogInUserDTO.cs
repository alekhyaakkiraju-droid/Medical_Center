using System.ComponentModel.DataAnnotations;

namespace AngularApi.Contracts.DTO
{
    public class LogInUserDTO
    {
        [Required]
        public string Email { get; set; }


        [Required]
        public string Password { get; set; }
    }
}
