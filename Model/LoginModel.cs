using System.ComponentModel.DataAnnotations;

namespace LimLink_API.Model
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }

        public string Password { get; set; }
        public string AccountType { get; set; }
    }
}
