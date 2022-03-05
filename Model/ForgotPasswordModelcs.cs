using System.ComponentModel.DataAnnotations;

namespace LimLink_API.Model
{
    public class ForgotPasswordModelcs
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
