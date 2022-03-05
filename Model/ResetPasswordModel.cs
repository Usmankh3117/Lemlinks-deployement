using System.ComponentModel.DataAnnotations;

namespace LimLink_API.Model
{
    public class ResetPasswordModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "New password")]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Re enter new password")]
        [Compare("Password", ErrorMessage = "Confirm password does not match")]
        public string ConfirmPassword { get; set; }

        public bool IsSuccess { get; set; }
    }
}
