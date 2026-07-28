using System.ComponentModel.DataAnnotations;

namespace AngularApi.Contracts.DTO
{
    public class LoginWith2FADTO
    {
        [Required]
        public string? TwoFactorCode { get; set; }
        public bool? RememberMe { get; set; }
        public bool? RememberMachine { get; set; }
    }

}
